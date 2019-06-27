using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Net.Messages;

namespace Mineral.Core.Net.Messages
{
    public class TransactionInventoryMessage : InventoryMessage
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public TransactionInventoryMessage(byte[] packed) : base(packed) { }
        public TransactionInventoryMessage(Protocol.Inventory inventory) : base(inventory) { }
        public TransactionInventoryMessage(List<SHA256Hash> hashes) : base(hashes, Protocol.Inventory.Types.InventoryType.Trx) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
