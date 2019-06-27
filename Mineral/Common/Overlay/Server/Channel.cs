using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Common.Overlay.Messages;
using Mineral.Core.Exception;

namespace Mineral.Common.Overlay.Server
{
    public class Channel
    {
        #region Field
        protected MessageQueue message_queue = new MessageQueue();
        private MessageCodec message_codec = new MessageCodec();
        //private NodeManager node_manager;
        //private StaticMessages static_messages;
        //private WireTrafficStats stats;
        //private HandShakeHandler handshake_handler;
        private P2pHandler p2p_handler;
        //private MineralNetHandler net_handler;

        protected NodeStatistics node_statistics;

        private ChannelManager channel_manager = null;
        private IChannelHandlerContext context = null;

        private volatile bool is_disconnect = true;
        #endregion


        #region Property
        public NodeStatistics Node
        {
            get { return this.node_statistics; }
        }

        public bool IsDisconnect
        {
            get { return this.is_disconnect; }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
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

        public void Close()
        {
            this.is_disconnect = true;
            this.p2p_handler.CloseAsync();
            this.message_queue.close();
            this.context.close();
        }
        #endregion
    }
}
