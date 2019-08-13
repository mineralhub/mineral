using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Codecs.Protobuf;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Common.Overlay.Messages;
using Mineral.Core;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Mineral.Core.Net;
using Mineral.Core.Net.Peer;
using Mineral.Utils;
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
        private IChannelHandlerContext context = null;
        private IPEndPoint socket_address = null;
        private PeerStatistics peer_statistics = new PeerStatistics();
        protected NodeStatistics node_statistics = null;
        protected HandShakeHandler handshake_handler = null;
        protected MessageCodec message_codec = new MessageCodec();
        protected MessageQueue message_queue = new MessageQueue();
        protected MineralNetHandler net_handler = new MineralNetHandler();
        protected P2pHandler p2p_handler = new P2pHandler();
        private MineralState state = MineralState.INIT;
        private Node node = null;

        private string remote_id = "";
        private long start_time = 0;
        private bool is_active = false;
        private bool is_trust_peer = false;
        private bool is_fast_forward_peer = false;
        private volatile bool is_disconnect = false;
        #endregion


        #region Property
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

        public long StartTime
        {
            get { return this.start_time; }
        }

        public bool IsActive
        {
            get { return this.is_active; }
        }

        public bool IsFastForwardPeer
        {
            get { return this.is_fast_forward_peer; }
        }
        #endregion


        #region Constructor
        public Channel()
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init(IChannelPipeline pipe_line, string remote_id, bool dicovery_mode)
        {
            this.handshake_handler = new HandShakeHandler(Manager.Instance.DBManager,
                                              Manager.Instance.NodeManager,
                                              Manager.Instance.ChannelManager,
                                              Manager.Instance.SyncPool);

            this.remote_id = remote_id;
            this.is_active = this.remote_id != null && this.remote_id.Length > 0;
            this.start_time = Helper.CurrentTimeMillis();

            pipe_line.AddLast("readTimeoutHandler", new ReadTimeoutHandler(60));
            pipe_line.AddLast(Manager.Instance.TrafficStats.TCP);
            pipe_line.AddLast("protoPender", new ProtobufVarint32LengthFieldPrepender());
            pipe_line.AddLast("lengthDecode", new ProtobufVarint32FrameDecoder());
            pipe_line.AddLast("handshakeHandler", this.handshake_handler);

            this.message_codec.Channel = this;
            this.message_queue.Channel = this;
            this.handshake_handler.Channel = this;
            this.handshake_handler.RemoteId = remote_id.IsNotNullOrEmpty() ? Helper.HexToBytes(remote_id) : new byte[0];
            this.p2p_handler.Channel = this;
            this.net_handler.Channel = this as PeerConnection;

            this.p2p_handler.MessageQueue = this.message_queue;
            this.net_handler.MessageQueue = this.message_queue;
        }

        public void InitNode(byte[] node_id, int remote_port)
        {
            this.node = new Node(node_id, this.socket_address.Address.MapToIPv4().ToString(), remote_port);
            this.node_statistics = Manager.Instance.NodeManager.GetNodeStatistics(node);
            Manager.Instance.NodeManager.GetNodeHandler(node).Node = node;
        }

        public void PublicHandshakeFinished(IChannelHandlerContext context, Messages.HelloMessage message)
        {
            this.is_trust_peer = Manager.Instance.ChannelManager.TrustNodes.ContainsKey(this.socket_address?.Address);
            this.is_fast_forward_peer = Manager.Instance.ChannelManager.FastForwardNodes.ContainsKey(this.socket_address?.Address);
            context.Channel.Pipeline.Remove(this.handshake_handler);

            this.message_queue.Activate(context);
            context.Channel.Pipeline.AddLast("message_codec", this.message_codec);
            context.Channel.Pipeline.AddLast("p2p", this.p2p_handler);
            context.Channel.Pipeline.AddLast("data", this.net_handler);

            this.start_time = message.Timestamp;
            this.state = MineralState.HANDSHAKE_FINISHED;
            this.node_statistics.P2pHandshake.Add();

            Logger.Info("Finish handshake with " + this.context.Channel.RemoteAddress);
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
            else if (exception is SocketException)
            {
                Logger.Error(string.Format("Close peer {0}", address));
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
            Manager.Instance.ChannelManager.ProcessDisconnect(this, reason);

            Messages.DisconnectMessage msg = new Messages.DisconnectMessage(reason);
            Logger.Info(
                string.Format("Send to {0} online-time {1}s, {2}",
                              this.context.Channel.RemoteAddress.ToString(),
                              (Helper.CurrentTimeMillis() - this.start_time) / 1000,
                              msg));

            this.node_statistics.NodeDisconnectedLocal(reason);

            Task task = this.context.WriteAndFlushAsync(msg.GetSendData());
            task.Wait();
            Close();
        }

        public void Close()
        {
            this.is_disconnect = true;
            this.p2p_handler.Close();
            this.message_queue.Close();
            Manager.Instance.ChannelManager.NotifyDisconnect(this);
            
            this.context.CloseAsync();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Channel channel = (Channel)obj;
            if (this.Address != null ? !this.Address.Equals(channel.Address) : channel.Address != null)
            {
                return false;
            }
            if (this.node != null ? !this.node.Equals(channel.node) : channel.node != null)
            {
                return false;
            }

            return this == channel;
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}", Address.ToString(), PeerId);
        }
        #endregion
    }
}
