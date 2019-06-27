using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Net.Messages;
using Mineral.Utils;
using Protocol;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Common.Overlay.Messages
{
    public class BlockInventoryMessage : MineralMessage
    {
        #region Field
        private Protocol.BlockInventory inventory = null;
        #endregion


        #region Property
        public Protocol.BlockInventory Inventory
        {
            get { return this.inventory; }
        }

        public List<BlockCapsule.BlockId> Ids
        {
            get { return this.inventory.Ids.Select(id => new BlockId(id.Hash, id.Number)).ToList(); }
        }
        #endregion


        #region Contructor
        public BlockInventoryMessage(byte[] raw_data)
            : base(raw_data)
        {
            this.type = (byte)MessageTypes.MsgType.BLOCK_INVENTORY;
            this.inventory = Protocol.BlockInventory.Parser.ParseFrom(data);
        }

        public BlockInventoryMessage(List<BlockId> ids, BlockInventory.Types.Type type)
        {
            this.inventory = new BlockInventory();
            ids.ForEach(id =>
            {
                BlockInventory.Types.BlockId block_id = new BlockInventory.Types.BlockId();
                block_id.Hash = id.Hash.ToByteString();
                block_id.Number = id.Num;
                this.inventory.Ids.Add(block_id);

            });

            this.inventory.Type = type;
            this.type = (byte)MessageTypes.MsgType.BLOCK_INVENTORY;
            this.data = inventory.ToByteArray();
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
