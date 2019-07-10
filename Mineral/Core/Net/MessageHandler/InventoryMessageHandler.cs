using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Core.Net.Service;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.MessageHandler
{
    public class InventoryMessageHandler : IMessageHandler
    {
        #region Field
        private MineralNetDelegate net_delegate = null;
        private AdvanceService advance_service = null;
        private TransactionMessageHandler handler_transaction = null;
        private int max_count = 10000;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool Check(PeerConnection peer, InventoryMessage message)
        {
            InventoryType type = message.InventoryType;
            int size = message.GetHashList().Count;

            if (peer.IsNeedSyncPeer || peer.IsNeedSyncUs)
            {
                Logger.Warning(
                    string.Format("Drop inv: {0} size: {1} from Peer {2}, syncFromUs: {3}, syncFromPeer: {4}.",
                                  type,
                                  size,
                                  peer.Address.ToString(),
                                  peer.IsNeedSyncUs,
                                  peer.IsNeedSyncPeer));

                return false;
            }

            if (type == InventoryType.Trx)
            {
                int count = peer.NodeStatistics.MessageStatistics.MineralInTrxInventory.GetCount(10);
                if (count > max_count)
                {
                    Logger.Warning(
                        string.Format("Drop inv: {0} size: {1} from Peer {2}, Inv count: {3} is overload.",
                                      type,
                                      size,
                                      peer.Address,
                                      count));

                    return false;
                }

                if (this.handler_transaction.IsBusy)
                {
                    Logger.Warning(
                        string.Format("Drop inv: {0} size: {1} from Peer {2}, transactionsMsgHandler is busy.",
                                      type,
                                      size,
                                      peer.Address));

                    return false;
                }
            }

            return true;
        }
        #endregion


        #region External Method
        public void ProcessMessage(PeerConnection peer, MineralMessage message)
        {
            InventoryMessage inventory_message = (InventoryMessage)message;
            InventoryType type = inventory_message.InventoryType;

            if (!Check(peer, inventory_message))
            {
                return;
            }

            foreach (SHA256Hash id in inventory_message.GetHashList())
            {
                Item item = new Item(id, type);
                peer.AddInventoryReceive(item, Helper.CurrentTimeMillis());
                this.advance_service.AddInventory(item);
            }
        }
        #endregion
    }
}
