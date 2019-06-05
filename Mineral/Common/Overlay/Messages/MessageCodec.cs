using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Server;

namespace Mineral.Common.Overlay.Messages
{
    public class MessageCodec : ByteToMessageDecoder
    {
        #region Field
        private Channel channel;
        private P2pMessageFactory p2p_message_factory = new P2pMessageFactory();
        private MineralMessageFactory mineral_message_factory = new MineralMessageFactory();
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
