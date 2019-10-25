using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Net.Udp.Message.Discover
{
    public class PongMessage : Message
    {
        #region Field
        private Protocol.PongMessage message = null;
        #endregion


        #region Property
        public int Version
        {
            get { return this.message.Echo; }
        }

        public override Node From
        {
            get
            {
                Protocol.Endpoint from = this.message.From;
                return new Node(from.NodeId.ToByteArray(),
                                Encoding.UTF8.GetString(from.Address.ToByteArray()),
                                from.Port);
            }
        }

        public override long Timestamp
        {
            get { return this.message.Timestamp; }
        }
        #endregion


        #region Contructor
        public PongMessage(byte[] data)
            : base(UdpMessageType.DISCOVER_PONG, data)
        {
            try
            {
                this.message = Protocol.PongMessage.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public PongMessage(Node from, long sequence)
            : base(UdpMessageType.DISCOVER_PONG, null)
        {
            Protocol.Endpoint endpoint_from = new Protocol.Endpoint();
            endpoint_from.NodeId = ByteString.CopyFrom(from.Id);
            endpoint_from.Port = from.Port;
            endpoint_from.Address = ByteString.CopyFrom(Encoding.UTF8.GetBytes(from.Host));

            this.message = new Protocol.PongMessage();
            this.message.From = endpoint_from;
            this.message.Echo = (int)Args.Instance.Node.P2P.Version;
            this.message.Timestamp = sequence;
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
            return "[pongMessage: " + message.ToString() + "]";
        }
        #endregion
    }
}
