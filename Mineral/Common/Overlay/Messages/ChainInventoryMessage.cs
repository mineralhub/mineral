using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Net.Messages;
using Mineral.Utils;

namespace Mineral.Common.Overlay.Messages
{
    public class ChainInventoryMessage : MineralMessage
    {
        #region Field
        protected Protocol.ChainInventory inventory = null;
        #endregion


        #region Property
        public Protocol.ChainInventory Inventory
        {
            get { return this.inventory; }
        }

        public List<BlockCapsule.BlockId> Ids
        {
            get { return this.inventory.Ids.Select(id => new BlockCapsule.BlockId(id.Hash, id.Number)).ToList(); }
        }

        public long RemainNumber
        {
            get { return this.inventory.RemainNum; }
        }
        #endregion


        #region Contructor
        public ChainInventoryMessage(byte[] data)
            : base(data)
        {
            try
            {
                this.type = (byte)MessageTypes.MsgType.BLOCK_CHAIN_INVENTORY;
                this.inventory = Protocol.ChainInventory.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public ChainInventoryMessage(List<BlockCapsule.BlockId> ids, long remain_num)
        {
            this.inventory = new Protocol.ChainInventory();
            ids.ForEach(id =>
            {
                Protocol.ChainInventory.Types.BlockId b = new Protocol.ChainInventory.Types.BlockId();
                b.Hash = id.Hash.ToByteString();
                b.Number = id.Num;
                this.inventory.Ids.Add(b);
            });

            this.inventory.RemainNum = remain_num;
            this.type = (byte)MessageTypes.MsgType.BLOCK_CHAIN_INVENTORY;
            this.data = this.inventory.ToByteArray();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append("size: ").Append(Ids.Count);
            if (Ids.Count >= 1)
            {
                sb.Append(", first blockId: ")
                  .Append(Ids[0].ToString());
                if (Ids.Count > 1)
                {
                    sb.Append(", end blockId: ")
                      .Append(Ids[Ids.Count - 1].ToString());
                }
            }
            return sb.ToString();
        }
        #endregion
    }
}
