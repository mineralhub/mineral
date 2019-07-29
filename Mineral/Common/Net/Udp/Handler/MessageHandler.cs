using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Mineral.Common.Net.Udp.Handler
{
    public class MessageHandler : SimpleChannelInboundHandler<UdpEvent>, IMessageHandler
    {
        #region Field
        private IChannel channel = null;
        private IEventHandler event_handler = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public MessageHandler(IDatagramChannel channel, IEventHandler event_handler)
        {
            this.channel = channel;
            this.event_handler = event_handler;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected override void ChannelRead0(IChannelHandlerContext context, UdpEvent udp_event)
        {
            Logger.Debug(
                string.Format("Receive udp message type {0}, length {1}, from {2}",
                              udp_event.Message.Type,
                              udp_event.Message.SendData.Length,
                              udp_event.Address));

            this.event_handler.HandlerEvent(udp_event);
        }
        #endregion


        #region External Method
        public override void ChannelActive(IChannelHandlerContext context)
        {
            this.event_handler.ChannelActivated();
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Info(
                string.Format("Exception caught, {0}, {1}",
                              context.Channel.RemoteAddress,
                              exception.Message));

            context.CloseAsync();
        }

        public void SendPacket(byte[] wire, IPEndPoint address)
        {
            DatagramPacket packet = new DatagramPacket(Unpooled.CopiedBuffer(wire), address);
            this.channel.WriteAsync(packet);
            this.channel.Flush();
        }

        public void Accept(UdpEvent udp_event)
        {
            Logger.Debug(
                string.Format("Send udp msg type {0}, length {1}, to {2} ",
                              udp_event.Message.Type,
                              udp_event.Message.SendData.Length,
                              udp_event.Address));

            IPEndPoint address = udp_event.Address;

            SendPacket(udp_event.Message.SendData, address);
        }
        #endregion
    }
}
