using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Net.Messages;

namespace Mineral.Common.Overlay.Messages
{
    public class PongMessage : P2pMessage
    {
        #region Field
        private static readonly byte[] FIXED_PAYLOAD = "C0".HexToBytes();
        #endregion


        #region Property
        public override byte[] Data => FIXED_PAYLOAD;
        public override MessageTypes.MsgType Type => (MessageTypes.MsgType)this.type;
        public override Type AnswerMessage
        {
            get { return GetType(); }
        }
        #endregion


        #region Contructor
        public PongMessage() : base((byte)MessageTypes.MsgType.P2P_PONG, FIXED_PAYLOAD) { }
        public PongMessage(byte type, byte[] raw_data) : base(type, raw_data) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion
    }
}
