using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Utils;

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
        private P2pMessage Create(byte type, byte[] raw_data)
        {
            P2pMessage result = null;
            MessageTypes.MsgType message_type = MessageTypes.FromByte(type);

            if (message_type == MessageTypes.MsgType.LAST)
            {
                throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, "type = " + type + ", len = " + raw_data.Length);
            }

            switch (message_type)
            {
                case MessageTypes.MsgType.P2P_HELLO:
                    {
                        result = new HelloMessage(type, raw_data);
                    }
                    break;
                case MessageTypes.MsgType.P2P_DISCONNECT:
                    {
                        result = new DisconnectMessage(type, raw_data);
                    }
                    break;
                case MessageTypes.MsgType.P2P_PING:
                    {
                        result = new PingMessage(type, raw_data);
                    }
                    break;
                case MessageTypes.MsgType.P2P_PONG:
                    {
                        result = new PongMessage(type, raw_data);
                    }
                    break;
                default:
                    {
                        throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, message_type.ToString());
                    }
            }

            return result;
        }
        #endregion


        #region External Method
        public override Message Create(byte[] data)
        {
            if (data.Length <= 1)
            {
                throw new P2pException(P2pException.ErrorType.MESSAGE_WITH_WRONG_LENGTH,
                                       "messageType = " + (data.Length == 1 ? data[0].ToString() : "Unknown"));
            }

            try
            {
                byte type = data[0];
                byte[] raw_data = ArrayUtil.SubArray(data, 1, data.Length);

                return Create(type, raw_data);
            }
            catch (System.Exception e)
            {
                if (e is P2pException)
                {
                    throw e;
                }
                else
                {
                    throw new P2pException(
                        P2pException.ErrorType.PARSE_MESSAGE_FAILED, "type=" + data[0] + ", len=" + data.Length);
                }
            }
        }
        #endregion
    }
}
