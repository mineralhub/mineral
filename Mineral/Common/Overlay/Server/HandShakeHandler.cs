using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Messages;
using Mineral.Core;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Net.Peer;
using static Mineral.Core.Net.Messages.MessageTypes;

namespace Mineral.Common.Overlay.Server
{
    public class HandShakeHandler : ByteToMessageDecoder
    {
        #region Field
        private byte[] remote_id = null;
        protected Channel channel = null;

        protected NodeManager node_manager = null;
        protected ChannelManager channel_manager = null;
        protected DatabaseManager db_manager = null;
        private SyncPool sync_pool = null;

        private P2pMessageFactory message_factory = new P2pMessageFactory();
        #endregion


        #region Property
        public byte[] RemoteId
        {
            get { return this.remote_id; }
            set { this.remote_id = value; }
        }

        public Channel Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }
        #endregion


        #region Contructor
        public HandShakeHandler(DatabaseManager db_manager,
                                NodeManager node_manager,
                                ChannelManager channel_manager,
                                SyncPool sync_pool)
        {
            this.db_manager = db_manager;
            this.node_manager = node_manager;
            this.channel_manager = channel_manager;
            this.sync_pool = sync_pool;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            byte[] encoded = new byte[input.ReadableBytes];
            input.ReadBytes(encoded);
            P2pMessage message = (P2pMessage)this.message_factory.Create(encoded);

            Logger.Info(
                string.Format("Handshake Receive from {0}, {1}",
                              context.Channel.RemoteAddress,
                              message));

            switch (message.Type)
            {
                case MsgType.P2P_HELLO:
                    HandleHelloMessage(context, (HelloMessage)message);
                    break;
                case MsgType.P2P_DISCONNECT:
                    if (this.channel.NodeStatistics!= null)
                    {
                        this.channel.NodeStatistics.NodeDisconnectedRemote(((DisconnectMessage)message).Reason);
                    }
                    this.channel.Close();
                    break;
                default:
                    this.channel.Close();
                    break;
            }

        }

        protected void SendHelloMessage(IChannelHandlerContext context, long time)
        {
            try
            {
                HelloMessage message = new HelloMessage(this.node_manager.PublicHomeNode,
                                                        time,
                                                        this.db_manager.GenesisBlockId,
                                                        this.db_manager.SolidBlockId,
                                                        this.db_manager.HeadBlockId);

                context.WriteAndFlushAsync(message.GetSendData());
                this.channel.NodeStatistics.MessageStatistics.AddTcpOutMessage(message);

                Logger.Info(
                       string.Format("Handshake Send to {0}, {1} ", context.Channel.RemoteAddress, message));
            }
            catch (System.Exception e)
            {
                Logger.Error("Fail to SendHelloMessage.", e);
            }
        }

        private void HandleHelloMessage(IChannelHandlerContext context, HelloMessage message)
        {
            this.channel.InitNode(message.From.Id, message.From.Port);

            if (this.remote_id.Length != 64)
            {
                IPAddress address = ((IPEndPoint)context.Channel.RemoteAddress).Address;
                if (!this.channel_manager.TrustNodes.ContainsKey(address) && !this.sync_pool.IsCanConnect)
                {
                    this.channel.Disconnect(Protocol.ReasonCode.TooManyPeers);
                    return;
                }
            }

            if (message.Version != Args.Instance.Node.P2P.Version)
            {
                Logger.Info(
                    string.Format("Peer {0} different p2p version, peer->{1}, me->{2}",
                                  context.Channel.RemoteAddress, message.Version, Args.Instance.Node.P2P.Version));

                this.channel.Disconnect(Protocol.ReasonCode.IncompatibleVersion);
                return;
            }

            if (!this.db_manager.GenesisBlockId.Hash.SequenceEqual(message.GenesisBlockId.Hash))
            {
                Logger.Info(
                    string.Format("Peer {0} different genesis block, peer->{1}, me->{2}",
                                  context.Channel.RemoteAddress,
                                  message.GenesisBlockId.ToString(),
                                  this.db_manager.GenesisBlockId.ToString()));

                this.channel.Disconnect(Protocol.ReasonCode.IncompatibleChain);
                return;
            }

            if (this.db_manager.SolidBlockId.Num >= message.SolidBlockId.Num
                && !this.db_manager.ContainBlockInMainChain(message.SolidBlockId))
            {
                Logger.Info(
                    string.Format("Peer {0} different solid block, peer->{1}, me->{2}",
                                  context.Channel.RemoteAddress,
                                  message.SolidBlockId.ToString(),
                                  this.db_manager.SolidBlockId.ToString()));

                this.channel.Disconnect(Protocol.ReasonCode.Forked);
                return;
            }

          ((PeerConnection)this.channel).HelloMessage = message;

            this.channel.NodeStatistics.MessageStatistics.AddTcpInMessage(message);
            this.channel.PublicHandshakeFinished(context, message);

            if (!this.channel_manager.ProcessPeer(channel))
            {
                this.channel.Disconnect(Protocol.ReasonCode.RecentlyDisconnected);
                return;
            }

            if (this.remote_id.Length != 64)
            {
                SendHelloMessage(context, message.Timestamp);
            }

            this.sync_pool.OnConnect(channel);
        }
        #endregion


        #region External Method
        public override void ChannelActive(IChannelHandlerContext context)
        {
            Logger.Info(
                string.Format("channel active, {0}", context.Channel.RemoteAddress));

            this.channel.Context = context;
            if (this.remote_id.Length == 64)
            {
                this.channel.InitNode(remote_id, ((IPEndPoint)context.Channel.RemoteAddress).Port);
                SendHelloMessage(context, Helper.CurrentTimeMillis());
            }
        }
        #endregion
    }
}
