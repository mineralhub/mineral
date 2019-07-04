using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Mineral.Common.Net.Udp.Handler
{
    public class PacketDecoder : MessageToMessageDecoder<DatagramPacket>
    {
        #region Field
        private static readonly int MAXSIZE = 2048;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected override void Decode(IChannelHandlerContext context, DatagramPacket message, List<object> output)
        {
            IByteBuffer buffer = message.Content;
            int length = buffer.ReadableBytes;

            if (length <= 1 || length >= MAXSIZE)
            {
                Logger.Error(
                    string.Format("UDP rcv bad packet, from {0} length = {1}",
                                  context.Channel.RemoteAddress,
                                  length));

                return;
            }

            byte[] encoded = new byte[length];
            buffer.ReadBytes(encoded);
            try
            {
                UdpEvent udp_event = new UdpEvent(Message.Message.Parse(encoded), (IPEndPoint)message.Sender);
                output.Add(udp_event);
            }
            catch (Exception e)
            {
                Logger.Error(
                    string.Format("Parse msg failed, type {0}, length {1}, address {2}", encoded[0], encoded.Length, message.Sender));
            }
        }
        #endregion


        #region External Method
        #endregion
    }
}
