using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Exception;
using Mineral.Core.Net.Message;

namespace Mineral.Common.Overlay.Messages
{
    public class P2pMessageFactory : MessageFactory
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private Message Create(byte type, byte[] raw_data)
        {
            MessageTypes.MsgType message_type = MessageTypes.FromByte(type);
            if (message_type == MessageTypes.MsgType.LAST)
                throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, "type = " + type + ", len = " + raw_data.Length);

            switch (message_type)
            {
                case MessageTypes.MsgType.P2P_HELLO:
                    return new HelloMessage
                case MessageTypes.MsgType.P2P_DISCONNECT:
                case MessageTypes.MsgType.P2P_PING:
                case MessageTypes.MsgType.P2P_PONG:
                default:
                    throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, message_type.ToString());
            }
        }
        #endregion


        #region External Method
        public override Message Create(byte[] data)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
