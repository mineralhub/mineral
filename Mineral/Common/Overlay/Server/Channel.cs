using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using DotNetty.Codecs.Protobuf;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Common.Overlay.Messages;
using Mineral.Core.Exception;
using Mineral.Core.Net;
using Mineral.Core.Net.Peer;
using Protocol;

namespace Mineral.Common.Overlay.Server
{
    public class Channel
    {
        public enum MineralState
        {
            INIT,
            HANDSHAKE_FINISHED,
            START_TO_SYNC,
            SYNCING,
            SYNC_COMPLETED,
            SYNC_FAILED
        }

        #region Field
        private static Channel instance = null;

        // working
        private NodeManager node_manager = NodeManager.Instance;
        private HandShakeHandler handshake_handler = new HandShakeHandler();
        //private StaticMessages static_messages;
        private ChannelManager channel_manager = null;
        private MineralState state = MineralState.INIT;


        // completed
        protected MessageQueue message_queue = new MessageQueue();
        private MessageCodec message_codec = new MessageCodec();
        private WireTrafficStats stats;
        private P2pHandler p2p_handler = new P2pHandler();
        private MineralNetHandler net_handler = new MineralNetHandler();
        protected NodeStatistics node_statistics;
        private PeerStatistics peer_statistics = new PeerStatistics();


        private IChannelHandlerContext context = null;

        private IPEndPoint socket_address = null;
        private Node node = null;
        private string remote_id = "";
        private long start_time = 0;
        private volatile bool is_disconnect = true;
        private bool is_active = false;
        private bool is_trust_peer = false;
        private bool is_fast_forward_peer = false;
        #endregion


        #region Property
        public static Channel Instance
        {
            get { return instance ?? new Channel(); }
        }

        public Node Node
        {
            get { return this.node; }
        }

        public NodeStatistics NodeStatistics
        {
            get { return this.node_statistics; }
        }

        public PeerStatistics PeerStatistics
        {
            get { return this.peer_statistics; }
        }

        public MineralState State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public bool IsDisconnect
        {
            get { return this.is_disconnect; }
        }

        public IChannelHandlerContext Context
        {
            get { return this.context; }
            set
            {
                this.context = value;
                this.socket_address = this.context != null ? (IPEndPoint)context.Channel.RemoteAddress : null;
            }
        }

        public IPAddress SocketAddress
        {
            get { return this.socket_address?.Address; }
        }

        public string PeerId
        {
            get { return this.node == null ? "<null>" : node.Id.ToHexString(); }
        }

        public IPAddress Address
        {
            get { return this.context == null ? null : ((IPEndPoint)(context.Channel.RemoteAddress)).Address; }
        }

        public bool IsFastForwardPeer
        {
            get { return this.is_fast_forward_peer; }
        }
        #endregion


        #region Constructor
        private Channel() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init(IChannelPipeline pipe_line, string remote_id, bool dicovery_mode, ChannelManager channel_manager)
        {
            this.channel_manager = channel_manager;
            this.remote_id = remote_id;
            this.is_active = this.remote_id != null && this.remote_id.Length > 0;
            this.start_time = Helper.CurrentTimeMillis();

            pipe_line.AddLast("readTimeoutHandler", new ReadTimeoutHandler(60));
            pipe_line.AddLast(this.stats.TCP);
            pipe_line.AddLast("protoPender", new ProtobufVarint32LengthFieldPrepender());
            pipe_line.AddLast("lengthDecode", new TxProtobufVarint32FrameDecoder(this));
            pipe_line.AddLast("handshakeHandler", this.handshake_handler);

            this.message_codec.Channel = this;
            this.message_queue.Channel = this;
            this.handshake_handler.Channel = this;
            this.handshake_handler.RemoteId = Encoding.UTF8.GetBytes(remote_id);
            this.p2p_handler.Channel = this;
            this.net_handler.Channel = this as PeerConnection;

            this.p2p_handler.MessageQueue = this.message_queue;
            this.net_handler.MessageQueue = this.message_queue;
        }

        public void InitNode(byte[] node_id, int remote_port)
        {
            this.node = new Node(node_id, this.socket_address.Address.ToString(), remote_port);
            this.node_statistics = this.node_manager.GetNodeStatistics(node);
            this.node_manager.GetNodeHandler(node).Node = node;
        }

        public void PublicHandshakeFinished(IChannelHandlerContext context, Messages.HelloMessage message)
        {
            this.is_trust_peer = this.channel_manager.TrustNodes.ContainsKey(this.socket_address?.Address);
            this.is_fast_forward_peer = this.channel_manager.FastForwardNodes.ContainsKey(this.socket_address?.Address);
            context.Channel.Pipeline.Remove(this.handshake_handler);

            this.message_queue.Activate(context);
            context.Channel.Pipeline.AddLast("messageCodec", this.message_codec);
            context.Channel.Pipeline.AddLast("p2p", this.p2p_handler);
            context.Channel.Pipeline.AddLast("data", this.net_handler);
            setStartTime(message.getTimestamp());
            setTronState(TronState.HANDSHAKE_FINISHED);
            getNodeStatistics().p2pHandShake.add();
            logger.info("Finish handshake with {}.", context.channel().remoteAddress());
        }

        public void ProcessException(System.Exception exception)
        {
            EndPoint address = this.context.Channel.RemoteAddress;
            if (exception is ReadTimeoutException || exception is IOException)
            {
                Logger.Warning(
                    string.Format("Close peer {0}, reason: {1}", address, exception.Message));
            }
            else if (exception is P2pException)
            {
                Logger.Warning(
                    string.Format("Close peer {0}, type: {1}, info: {2}",
                                  address,
                                  ((P2pException)exception).Type,
                                  exception.Message));
            }
            else
            {
                Logger.Error(string.Format("Close peer {0}, exception caught", address));
            }

            Close();
        }

        public void Disconnect(ReasonCode reason)
        {
            this.is_disconnect = true;
            this.channel_manager.processDisconnect(this, reason);
            DisconnectMessage msg = new DisconnectMessage(reason);
            logger.info("Send to {} online-time {}s, {}",
                ctx.channel().remoteAddress(),
                (System.currentTimeMillis() - startTime) / 1000,
                msg);
            getNodeStatistics().nodeDisconnectedLocal(reason);
            ctx.writeAndFlush(msg.getSendData()).addListener(future->close());
        }

        public void Close()
        {
            this.is_disconnect = true;
            this.p2p_handler.Close();
            this.message_queue.Close();
            this.context.CloseAsync();
        }
        #endregion
    }
}
