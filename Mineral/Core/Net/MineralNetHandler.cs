using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Server;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;

namespace Mineral.Core.Net
{
    public class MineralNetHandler : SimpleChannelInboundHandler<MineralMessage>
    {
        #region Field
        protected PeerConnection peer = null;
        private MessageQueue message_queue = null;
        #endregion


        #region Property
        public MessageQueue MessageQueue
        {
            get { return this.message_queue; }
            set { this.message_queue = value; }
        }

        public PeerConnection Channel
        {
            get { return this.peer; }
            set { this.peer = value; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected override void ChannelRead0(IChannelHandlerContext ctx, MineralMessage msg)
        {
            this.message_queue.ReceivedMessage(msg);
            Manager.Instance.NetService.OnMessage(this.peer, msg);
        }
        #endregion


        #region External Method
        public override void ExceptionCaught(IChannelHandlerContext context, System.Exception exception)
        {
            this.peer.ProcessException(exception);
        }
        #endregion
    }
}
