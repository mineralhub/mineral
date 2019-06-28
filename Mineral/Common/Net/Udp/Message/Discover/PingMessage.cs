using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Net.Udp.Message.Discover
{
    public class PingMessage : Message
    {
        #region Field
        private Protocol.PingMessage message = null;
        #endregion


        #region Property
        public int Version
        {
            get { return this.message.Version; }
        }

        public Node To
        {
            get
            {
                Protocol.Endpoint to = this.message.To;
                return new Node(to.NodeId.ToByteArray(),
                                Encoding.UTF8.GetString(to.Address.ToByteArray()),
                                to.Port);
            }
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
        public PingMessage(byte[] data)
            : base(UdpMessageType.DISCOVER_PING, data)
        {
            try
            {
                this.message = Protocol.PingMessage.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public PingMessage(Node from, Node to)
            : base(UdpMessageType.DISCOVER_PING, null)
        {
            Protocol.Endpoint endpoint_from = new Protocol.Endpoint();
            endpoint_from.NodeId = ByteString.CopyFrom(from.Id);
            endpoint_from.Port = from.Port;
            endpoint_from.Address = ByteString.CopyFrom(Encoding.UTF8.GetBytes(from.Host));

            Protocol.Endpoint endpoint_to = new Protocol.Endpoint();
            endpoint_to.NodeId = ByteString.CopyFrom(to.Id);
            endpoint_to.Port = to.Port;
            endpoint_to.Address = ByteString.CopyFrom(Encoding.UTF8.GetBytes(to.Host)));

            this.message = new Protocol.PingMessage();
            this.message.Version = (int)Args.Instance.Node.P2P.Version;
            this.message.From = endpoint_from;
            this.message.To = endpoint_to;
            this.message.Timestamp = DateTime.Now.Ticks;
            this.data = this.message.ToByteArray();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
