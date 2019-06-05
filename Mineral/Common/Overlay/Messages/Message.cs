using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using Google.Protobuf;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Cryptography;
using static Mineral.Core.Net.Messages.MessageTypes;

namespace Mineral.Common.Overlay.Messages
{
    public abstract class Message
    {
        #region Field
        protected byte[] data = null;
        protected byte type = 0x00;
        private static Manager db_manager;
        #endregion


        #region Property
        public byte[] Data { get { return this.data; } }
        public MsgType Type { get { return MessageTypes.FromByte(this.type); } }
        public static bool IsFilter { get { return db_manager.DynamicProperties.GetAllowProtoFilterNum() == 1; } }
        #endregion


        #region Constructor
        public Message() { }

        public Message(byte[] packed)
        {
            this.data = packed;
        }

        public Message(byte type, byte[] packed)
        {
            this.type = type;
            this.data = packed;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public IByteBuffer GetSendData()
        {
            byte[] result = null;
            if (this.data == null)
            {
                result = new byte[1] { this.type };
            }
            else
            {
                result = new byte[this.data.Length + 1];
                Array.Copy(this.data, result, this.data.Length);
                result[this.data.Length + 1] = type;
            }

            return Unpooled.WrappedBuffer(result);
        }

        public byte[] GetMessageId()
        {
            return this.data.SHA256();
        }

        public static void CompareBytes(byte[] src, byte[] dst)
        {
            if (src.Length != dst.Length)
            {
                throw new P2pException(
                    P2pException.ErrorType.PROTOBUF_ERROR,
                    P2pException.GetDescription(P2pException.ErrorType.PROTOBUF_ERROR)
                    );
            }
        }

        public static CodedInputStream GetCodedInputStream(byte[] data)
        {
            return new CodedInputStream(data);
        }

        public override string ToString()
        {
            return "type:" + Type.ToString() + "\n";
        }
        #endregion
    }
}
