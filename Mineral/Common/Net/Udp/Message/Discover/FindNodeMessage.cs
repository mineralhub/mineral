using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Overlay.Discover.Node;

namespace Mineral.Common.Net.Udp.Message.Discover
{
    public class FindNodeMessage : Message
    {
        #region Field
        private Protocol.FindNeighbours message = null;
        #endregion


        #region Property
        public byte[] TargetId
        {
            get { return this.message.TargetId.ToByteArray(); }
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
        public FindNodeMessage(byte[] data)
            : base(UdpMessageType.DISCOVER_FIND_NODE, data)
        {
            try
            {
                this.message = Protocol.FindNeighbours.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }

        }

        public FindNodeMessage(Node from, byte[] target_id)
            : base(UdpMessageType.DISCOVER_FIND_NODE, null)
        {

            Protocol.Endpoint endpoint_from = new Protocol.Endpoint();
            endpoint_from.NodeId = ByteString.CopyFrom(from.Id);
            endpoint_from.Port = from.Port;
            endpoint_from.Address = ByteString.CopyFrom(Encoding.UTF8.GetBytes(from.Host));

            this.message = new Protocol.FindNeighbours();
            this.message.From = endpoint_from;
            this.message.TargetId = ByteString.CopyFrom(target_id);
            this.message.Timestamp = DateTime.Now.Ticks;
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
            return "[findNeighbours: " + this.message.ToString() + "]";
        }
        #endregion
    }
}
