using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Mineral.Common.Overlay.Server
{
    public class TrafficStatHandler : ChannelDuplexHandler
    {
        #region Field
        private long in_size = 0;
        private long out_size = 0;
        private long in_packets = 0;
        private long out_packets = 0;
        #endregion


        #region Property
        public override bool IsSharable
        {
            get { return true; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public string Stats()
        {
            return "";
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Interlocked.Increment(ref this.in_packets);

            if (message is IByteBuffer)
            {
                Interlocked.Exchange(ref this.in_size, ((IByteBuffer)message).ReadableBytes);
            }
            else
            {
                Interlocked.Exchange(ref this.in_size, ((DatagramPacket)message).Content.ReadableBytes);
            }

            base.ChannelRead(context, message);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            Interlocked.Increment(ref this.out_packets);

            if (message is IByteBuffer)
            {
                Interlocked.Exchange(ref this.out_size, ((IByteBuffer)message).ReadableBytes);
            }
            else
            {
                Interlocked.Exchange(ref this.out_size, ((DatagramPacket)message).Content.ReadableBytes);
            }

            return base.WriteAsync(context, message);
        }
        #endregion
    }
}
