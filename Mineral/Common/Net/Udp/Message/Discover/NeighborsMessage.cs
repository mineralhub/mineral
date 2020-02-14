using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Overlay.Discover.Node;

namespace Mineral.Common.Net.Udp.Message.Discover
{
    public class NeighborsMessage : Message
    {
        #region Field
        private Protocol.Neighbours message = null;
        #endregion


        #region Property
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

        public List<Node> Nodes
        {
            get
            {
                List<Node> nodes = new List<Node>();
                foreach (var neighbour in this.message.Neighbours_)
                {
                    nodes.Add(new Node(neighbour.NodeId.ToByteArray(),
                                       Encoding.UTF8.GetString(neighbour.Address.ToByteArray()),
                                       neighbour.Port));
                }

                return nodes;
            }
        }
        #endregion


        #region Contructor
        public NeighborsMessage(byte[] data)
            : base(UdpMessageType.DISCOVER_NEIGHBORS, data)
        {
            try
            {
                this.message = Protocol.Neighbours.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public NeighborsMessage(Node from, List<Node> neighbours, long sequence)
            : base(UdpMessageType.DISCOVER_NEIGHBORS, null)
        {
            this.message = new Protocol.Neighbours();
            this.message.Timestamp = sequence;

            neighbours.ForEach(neighbour =>
            {
                Protocol.Endpoint endpoint = new Protocol.Endpoint();
                endpoint.NodeId = ByteString.CopyFrom(neighbour.Id);
                endpoint.Port = neighbour.Port;
                endpoint.Address = ByteString.CopyFrom(Encoding.UTF8.GetBytes(neighbour.Host));

                this.message.Neighbours_.Add(endpoint);
            });

            this.message.From = new Protocol.Endpoint();
            this.message.From.NodeId = ByteString.CopyFrom(from.Id);
            this.message.From.Port = from.Port;
            this.message.From.Address = ByteString.CopyFrom(Encoding.UTF8.GetBytes(from.Host));
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
            return "[neighbours: " + this.message + "]";
        }
        #endregion
    }
}
