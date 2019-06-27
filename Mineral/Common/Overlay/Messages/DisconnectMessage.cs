using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Net.Messages;

namespace Mineral.Common.Overlay.Messages
{
    public class DisconnectMessage : P2pMessage
    {
        #region Field
        private Protocol.DisconnectMessage message = null;
        #endregion


        #region Property
        public Protocol.ReasonCode Reason
        {
            get { return this.message.Reason; }
        }
        #endregion


        #region Contructor
        public DisconnectMessage(byte type, byte[] raw_data)
            : base(type, raw_data)
        {
            try
            {
                this.message = Protocol.DisconnectMessage.Parser.ParseFrom(raw_data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public DisconnectMessage(Protocol.ReasonCode code)
        {
            this.message = new Protocol.DisconnectMessage();
            this.message.Reason = code;
            this.type = (byte)MessageTypes.MsgType.P2P_DISCONNECT;
            this.data = this.message.ToByteArray();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            return new StringBuilder().Append(base.ToString()).Append("reason: ").Append(this.message.Reason).ToString();
        }
        #endregion
    }
}
