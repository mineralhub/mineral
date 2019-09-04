using System;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Net.Messages;
using Protocol;

namespace Mineral.Common.Overlay.Messages
{
    using Node = Mineral.Common.Overlay.Discover.Node.Node;

    public class HelloMessage : P2pMessage
    {
        #region Field
        private Protocol.HelloMessage message = null;
        #endregion


        #region Property
        public int Version
        {
            get { return this.message.Version; }
        }

        public long Timestamp
        {
            get { return this.message.Timestamp; }
        }

        public Node From
        {
            get
            {
                Endpoint from = this.message.From;
                return new Node(from.NodeId.ToByteArray(), Encoding.UTF8.GetString(from.Address.ToByteArray()), from.Port);
            }
        }

        public BlockCapsule.BlockId GenesisBlockId
        {
            get { return new BlockCapsule.BlockId(this.message.GenesisBlockId.Hash, this.message.GenesisBlockId.Number); }
        }

        public BlockCapsule.BlockId SolidBlockId
        {
            get { return new BlockCapsule.BlockId(this.message.SolidBlockId.Hash, this.message.SolidBlockId.Number); }
        }

        public BlockCapsule.BlockId HeadBlockId
        {
            get { return new BlockCapsule.BlockId(this.message.HeadBlockId.Hash, this.message.HeadBlockId.Number); }
        }

        public override Type AnswerMessage
        {
            get { return null; }
        }
        #endregion


        #region Constructor
        public HelloMessage(byte type, byte[] raw_data)
            : base(type, raw_data)
        {
            this.message = Protocol.HelloMessage.Parser.ParseFrom(raw_data);
        }

        public HelloMessage(Node node,
                            long timestamp,
                            BlockCapsule.BlockId genesis_block,
                            BlockCapsule.BlockId solid_block,
                            BlockCapsule.BlockId head_block)
        {
            Endpoint from = new Endpoint();
            from.NodeId = ByteString.CopyFrom(node.Id);
            from.Port = node.Port;
            from.Address = ByteString.CopyFrom(Encoding.UTF8.GetBytes(node.Host));

            Protocol.HelloMessage.Types.BlockId genesis = new Protocol.HelloMessage.Types.BlockId();
            genesis.Hash = ByteString.CopyFrom(genesis_block.Hash);
            genesis.Number = genesis_block.Num;

            Protocol.HelloMessage.Types.BlockId solid = new Protocol.HelloMessage.Types.BlockId();
            solid.Hash = ByteString.CopyFrom(solid_block.Hash);
            solid.Number = solid_block.Num;

            Protocol.HelloMessage.Types.BlockId head = new Protocol.HelloMessage.Types.BlockId();
            head.Hash = ByteString.CopyFrom(head_block.Hash);
            head.Number = head_block.Num;

            this.message = new Protocol.HelloMessage();
            this.message.From = from;
            this.message.Version = (int)Args.Instance.Node.P2P.Version;
            this.message.Timestamp = timestamp;
            this.message.GenesisBlockId = genesis;
            this.message.SolidBlockId = solid;
            this.message.HeadBlockId = head;

            this.type = (byte)MessageTypes.MsgType.P2P_HELLO;
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
            return new StringBuilder().Append(base.ToString()).Append(this.message.ToString()).ToString();
        }
        #endregion
    }
}
