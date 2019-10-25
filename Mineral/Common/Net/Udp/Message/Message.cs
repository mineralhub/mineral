using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Net.Udp.Message.Backup;
using Mineral.Common.Net.Udp.Message.Discover;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Utils;
using Mineral.Core.Exception;
using Mineral.Utils;

namespace Mineral.Common.Net.Udp.Message
{
    public abstract class Message
    {
        #region Field
        protected byte[] data = null;
        protected UdpMessageType type = UdpMessageType.UNKNOWN;
        #endregion


        #region Property
        public abstract Node From { get; }
        public abstract long Timestamp { get; }

        public byte[] Data
        {
            get { return this.data; }
        }

        public byte[] SendData
        {
            get
            {
                byte[] result = null;
                if (this.data == null)
                {
                    result = new byte[1] { (byte)this.type };
                }
                else
                {
                    result = new byte[this.data.Length + 1];
                    result[0] = (byte)this.type;
                    Array.Copy(this.data, 0, result, 1, this.data.Length);
                }

                return result;
            }
        }

        public UdpMessageType Type
        {
            get { return this.type; }
        }

        public SHA256Hash MessageId
        {
            get { return SHA256Hash.Of(data); }
        }
        #endregion


        #region Contructor
        public Message(UdpMessageType type, byte[] data)
        {
            this.type = type;
            this.data = data;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static Message Parse(byte[] encode)
        {
            byte type = encode[0];
            byte[] data = ArrayUtil.SubArray(encode, 1, encode.Length);

            if (Enum.IsDefined(typeof(UdpMessageType), type))
            {
                switch ((UdpMessageType)type)
                {
                    case UdpMessageType.DISCOVER_PING:
                        return new PingMessage(data);
                    case UdpMessageType.DISCOVER_PONG:
                        return new PongMessage(data);
                    case UdpMessageType.DISCOVER_FIND_NODE:
                        return new FindNodeMessage(data);
                    case UdpMessageType.DISCOVER_NEIGHBORS:
                        return new NeighborsMessage(data);
                    case UdpMessageType.BACKUP_KEEP_ALIVE:
                        return new KeepAliveMessage(data);
                    default:
                        throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, "type=" + type);
                }
            }
            else
            {
                throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, "type=" + type);
            }
        }

        public override string ToString()
        {
            return "[Message Type: " + this.type + ", length: " + (this.data == null ? 0 : this.data.Length) + "]";
        }
        #endregion
    }
}
