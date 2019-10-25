using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.Peer
{
    public class Item
    {
        #region Field
        private SHA256Hash hash = null;
        private InventoryType type = InventoryType.Block;
        private long time = 0;
        #endregion


        #region Property
        public SHA256Hash Hash
        {
            get { return this.hash; }
        }

        public InventoryType Type
        {
            get { return this.type; }
        }

        public long Time
        {
            get { return this.time; }
        }
        #endregion


        #region Contructor
        public Item(SHA256Hash hash, InventoryType type)
        {
            this.hash = hash;
            this.type = type;
            this.time = Helper.CurrentTimeMillis();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            Item item = (Item)obj;

            return this.hash.Equals(item.Hash) && type.Equals(item.Type);
        }

        public override int GetHashCode()
        {
            return this.hash.GetHashCode();
        }

        public override string ToString()
        {
            return this.type + ":" + this.hash;
        }
        #endregion
    }
}
