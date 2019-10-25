using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Server;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;

namespace Mineral.Common.Overlay.Messages
{
    public class MessageCodec : ByteToMessageDecoder
    {
        #region Field
        private Channel channel;
        private P2pMessageFactory p2p_message = new P2pMessageFactory();
        private MineralMessageFactory mineral_message = new MineralMessageFactory();
        #endregion


        #region Property
        public Channel Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private Message CreateMessage(byte[] encoded)
        {
            byte type = encoded[0];
            if (MessageTypes.IsP2p(type))
            {
                return this.p2p_message.Create(encoded);
            }

            if (MessageTypes.IsMineral(type))
            {
                return this.mineral_message.Create(encoded);
            }

            throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, "type=" + encoded[0]);
        }
        #endregion


        #region External Method
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            int length = input.ReadableBytes;
            byte[] encoded = new byte[length];

            input.ReadBytes(encoded);
            try
            {
                Message msg = CreateMessage(encoded);
                this.channel.NodeStatistics.TcpFlow.Add(length);
                output.Add(msg);
            }
            catch (Exception e)
            {
                this.channel.ProcessException(e);
            }
        }
        #endregion
    }
}
