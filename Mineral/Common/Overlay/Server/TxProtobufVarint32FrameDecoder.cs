using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace Mineral.Common.Overlay.Server
{
    public class TxProtobufVarint32FrameDecoder : ByteToMessageDecoder
    {
        #region Field
        private Channel channel = null;
        private static readonly int MAX_MESSAGE_LENGTH = 5 * 1024 * 1024;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public TxProtobufVarint32FrameDecoder(Channel channel)
        {
            this.channel = channel;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static int ReadRawVarint32(IByteBuffer buffer)
        {
            if (!buffer.IsReadable())
            {
                return 0;
            }
            buffer.MarkReaderIndex();
            byte tmp = buffer.ReadByte();
            if (tmp >= 0)
            {
                return tmp;
            }
            else
            {
                int result = tmp & 127;
                if (!buffer.IsReadable())
                {
                    buffer.ResetReaderIndex();
                    return 0;
                }
                if ((tmp = buffer.ReadByte()) >= 0)
                {
                    result |= tmp << 7;
                }
                else
                {
                    result |= (tmp & 127) << 7;
                    if (!buffer.IsReadable())
                    {
                        buffer.ResetReaderIndex();
                        return 0;
                    }
                    if ((tmp = buffer.ReadByte()) >= 0)
                    {
                        result |= tmp << 14;
                    }
                    else
                    {
                        result |= (tmp & 127) << 14;
                        if (!buffer.IsReadable())
                        {
                            buffer.ResetReaderIndex();
                            return 0;
                        }
                        if ((tmp = buffer.ReadByte()) >= 0)
                        {
                            result |= tmp << 21;
                        }
                        else
                        {
                            result |= (tmp & 127) << 21;
                            if (!buffer.IsReadable())
                            {
                                buffer.ResetReaderIndex();
                                return 0;
                            }
                            result |= (tmp = buffer.ReadByte()) << 28;
                            if (tmp < 0)
                            {
                                throw new CorruptedFrameException("malformed varint.");
                            }
                        }
                    }
                }
                return result;
            }
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            input.MarkReaderIndex();
            int pre_index = input.ReaderIndex;
            int length = ReadRawVarint32(input);
            if (length >= MAX_MESSAGE_LENGTH)
            {
                Logger.Error(
                    string.Format("recv a big msg, host : {0}, msg length is : {1}",
                                  context.Channel.RemoteAddress,
                                  length));

                input.Clear();
                channel.Close();

                return;
            }
            if (pre_index == input.ReaderIndex)
            {
                return;
            }
            if (length < 0)
            {
                throw new CorruptedFrameException("negative length: " + length);
            }

            if (input.ReadableBytes < length)
            {
                input.ResetReaderIndex();
            }
            else
            {
                output.Add(input.ReadRetainedSlice(length));
            }
        }
        #endregion


        #region External Method
        #endregion
    }
}
