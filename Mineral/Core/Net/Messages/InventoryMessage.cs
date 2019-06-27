using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Utils;
using Protocol;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.Messages
{
    public class InventoryMessage : MineralMessage
    {
        #region Field
        protected Inventory inventory = null;
        #endregion


        #region Property
        public Inventory Inventory
        {
            get { return this.inventory; }
        }

        public InventoryType InventoryType
        {
            get { return this.inventory.Type; }
        }

        public MessageTypes.MsgType InventoryMessageType 
        {
            get { return this.inventory.Type.Equals(InventoryType.Block) ? MessageTypes.MsgType.BLOCK : MessageTypes.MsgType.TX; }
        }
        #endregion


        #region Constructor
        public InventoryMessage(byte[] data)
            : base(data)
        {
            this.type = (byte)MessageTypes.MsgType.INVENTORY;
            this.inventory = Inventory.Parser.ParseFrom(data);
        }

        public InventoryMessage(Inventory inventory)
        {
            this.inventory = inventory;
            this.type = (byte)MessageTypes.MsgType.INVENTORY;
            this.data = inventory.ToByteArray();
        }

        public InventoryMessage(List<SHA256Hash> hashes, InventoryType type)
        {
            this.inventory = new Inventory();
            foreach (SHA256Hash hash in hashes)
            {
                this.inventory.Ids.Add(ByteString.CopyFrom(hash.Hash));
            }
            this.type = (byte)MessageTypes.MsgType.INVENTORY;
            this.data = inventory.ToByteArray();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public List<SHA256Hash> GetHashList()
        {
            return this.inventory.Ids.Select(hash => SHA256Hash.Wrap(hash.ToByteArray())).ToList();
        }

        public override string ToString()
        {
            List<SHA256Hash> hashes = GetHashList();

            StringBuilder builder = new StringBuilder();
            builder
                .Append(base.ToString())
                .Append("inventory type: ").Append(InventoryType)
                .Append(", size: ").Append(hashes.Count)
                .Append(", first hash: ").Append(hashes.First());
            if (hashes.Count > 1)
            {
                builder.Append(", end hash: ").Append(hashes.Last());
            }
            return builder.ToString();
        }
        #endregion
    }
}
