using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Common.Overlay.Messages;
using Mineral.Core.Net.Messages;
using Mineral.Utils;
using static Mineral.Utils.ScheduledExecutorService;

namespace Mineral.Common.Overlay.Server
{
    public class P2pHandler : SimpleChannelInboundHandler<P2pMessage>
    {
        #region Field
        private ScheduledExecutorHandle timer_ping = null;
        private MessageQueue message_quque = null;
        private Channel channel = null;

        private volatile bool has_ping = false;
        private long send_ping_time = 0;
        #endregion


        #region Property
        public Channel Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }

        public MessageQueue MessageQueue
        {
            get { return this.message_quque; }
            set { this.message_quque = value; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected override void ChannelRead0(IChannelHandlerContext context, P2pMessage message)
        {
            Logger.Debug(
                string.Format("Receive message : {0}", message.Type.ToString()));

            this.message_quque.ReceivedMessage(message);
            MessageStatistics message_statistics = this.channel.NodeStatistics.MessageStatistics;
            switch (message.Type)
            {
                case MessageTypes.MsgType.P2P_PING:
                    {
                        int count = message_statistics.P2pInPing.GetCount(10);
                        if (count > 3)
                        {
                            string reason = string.Format("TCP attack found: {0} with ping count({1})",
                                                          context.Channel.RemoteAddress,
                                                          count);
                            Logger.Warning(reason);
                            this.channel.Disconnect(Protocol.ReasonCode.BadProtocol, reason);

                            return;
                        }

                        this.message_quque.SendMessage(new PongMessage());
                    }
                    break;
                case MessageTypes.MsgType.P2P_PONG:
                    {
                        if (message_statistics.P2pInPong.TotalCount > message_statistics.P2pOutPing.TotalCount)
                        {
                            string reason = string.Format("TCP attack found: {0} with ping count({1}), pong count({2})",
                                                          context.Channel.RemoteAddress,
                                                          message_statistics.P2pOutPing.TotalCount,
                                                          message_statistics.P2pInPong.TotalCount);
                            Logger.Warning(reason);
                            this.channel.Disconnect(Protocol.ReasonCode.BadProtocol, reason);

                            return;
                        }

                        this.has_ping = false;
                        this.channel.NodeStatistics.LastPongReplyTime = Helper.CurrentTimeMillis();
                        this.channel.PeerStatistics.Pong(this.send_ping_time);
                    }
                    break;
                case MessageTypes.MsgType.P2P_DISCONNECT:
                    {
                        this.channel.NodeStatistics.NodeDisconnectedRemote(((DisconnectMessage)message).Reason);
                        this.channel.Close();
                    }
                    break;
                default:
                    {
                        this.channel.Close();
                    }
                    break;
            }
        }
        #endregion


        #region External Method
        public override void HandlerAdded(IChannelHandlerContext context)
        {
            this.timer_ping = ScheduledExecutorService.Scheduled(() =>
            {
                if (!this.has_ping)
                {
                    this.send_ping_time = Helper.CurrentTimeMillis();
                    this.has_ping = this.message_quque.SendMessage(new PingMessage());
                }
            }, 10 * 1000, 10 * 1000);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            this.channel.ProcessException(exception);
        }

        public void Close()
        {
            if (this.timer_ping != null && !this.timer_ping.IsCanceled)
            {
                this.timer_ping.Cancel();
                this.timer_ping = null;
            }
        }
        #endregion
    }
}
