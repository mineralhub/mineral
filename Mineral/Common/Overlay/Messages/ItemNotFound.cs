using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Net.Messages;

namespace Mineral.Common.Overlay.Messages
{
    public class ItemNotFoundMessage : MineralMessage
    {
        #region Field
        private Protocol.Items item = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ItemNotFoundMessage()
        {
            this.item = new Protocol.Items();
            this.item.Type = Protocol.Items.Types.ItemType.Err;
            this.type = (byte)MessageTypes.MsgType.ITEM_NOT_FOUND;
            this.data = this.item.ToByteArray();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            return "Item not found.";
        }
        #endregion
    }
}
