using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Net.Messages;

namespace Mineral.Core.Net.Messages
{
    public class FetchBlockHeadersMessage : InventoryMessage
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public FetchBlockHeadersMessage(byte[] packed)
            : base(packed)
        {
            this.type = (byte)MessageTypes.MsgType.FETCH_BLOCK_HEADERS;
        }

        public FetchBlockHeadersMessage(Protocol.Inventory inventory)
            : base(inventory)
        {
            this.type = (byte)MessageTypes.MsgType.FETCH_BLOCK_HEADERS;
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
