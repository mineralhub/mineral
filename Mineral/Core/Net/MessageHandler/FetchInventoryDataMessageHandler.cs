using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Utils;
using Mineral.Core.Config;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Core.Net.Service;
using Protocol;
using static Mineral.Core.Capsule.BlockCapsule;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.MessageHandler
{
    public class FetchInventoryDataMessageHandler : IMessageHandler
    {
        #region Field
        private MineralNetDelegate net_delegate = null;
        private SyncService sync_service = null;
        private AdvanceService advance_service = null;
        private int MAX_SIZE = 1000000;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Check(PeerConnection peer, FetchInventoryDataMessage message)
        {
            MessageTypes.MsgType type = message.InventoryMessageType;

            if (type == MessageTypes.MsgType.TX)
            {
                foreach (SHA256Hash hash in message.GetHashList())
                {
                    if (peer.GetInventorySpread(new Item(hash, InventoryType.Trx)) == null)
                    {
                        throw new P2pException(P2pException.ErrorType.BAD_MESSAGE, "not spread inv : " + hash);
                    }
                }

                int fetch_count = peer.NodeStatistics.MessageStatistics.MineralInTrxFetchInvDataElement.GetCount(10);
                int max_count = this.advance_service.TxCount.GetCount(60);
                if (fetch_count > max_count)
                {
                    throw new P2pException(
                        P2pException.ErrorType.BAD_MESSAGE, "maxCount: " + max_count + ", fetchCount: " + fetch_count);
                }
            }
            else
            {
                bool is_advance = true;
                foreach (SHA256Hash hash in message.GetHashList())
                {
                    if (peer.GetInventorySpread(new Item(hash, InventoryType.Block)) == null)
                    {
                        is_advance = false;
                        break;
                    }
                }

                if (is_advance)
                {
                    MessageCount out_advance_block = peer.NodeStatistics.MessageStatistics.MineralOutAdvBlock;
                    out_advance_block.Add(message.GetHashList().Count);

                    int out_block_count_1min = out_advance_block.GetCount(60);
                    int produced_block_2min = 120000 / Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;
                    if (out_block_count_1min > produced_block_2min)
                    {
                        throw new P2pException(
                            P2pException.ErrorType.BAD_MESSAGE,
                            "producedBlockIn2min: " + produced_block_2min + ", outBlockCountIn1min: " + out_block_count_1min);
                    }
                }
                else
                {
                    if (!peer.IsNeedSyncUs)
                    {
                        throw new P2pException(
                            P2pException.ErrorType.BAD_MESSAGE, "no need sync");
                    }

                    foreach (SHA256Hash hash in message.GetHashList())
                    {
                        long block_num = new BlockId(hash).Num;
                        long min_block_num = peer.LastSyncBlockId.Num - 2 * Parameter.NodeParameters.SYNC_FETCH_BATCH_NUM;

                        if (block_num < min_block_num)
                        {
                            throw new P2pException(
                                P2pException.ErrorType.BAD_MESSAGE, "minBlockNum: " + min_block_num + ", blockNum: " + block_num);
                        }

                        if (peer.GetSyncBlockId(hash) != null)
                        {
                            throw new P2pException(
                                P2pException.ErrorType.BAD_MESSAGE, new BlockId(hash).GetString() + " is exist");
                        }

                        peer.AddSyncBlockId(hash, Helper.CurrentTimeMillis());
                    }
                }
            }
        }
        #endregion


        #region External Method
        public void ProcessMessage(PeerConnection peer, Messages.MineralMessage message)
        {
            FetchInventoryDataMessage fetch_message = (FetchInventoryDataMessage)message;

            Check(peer, fetch_message);

            InventoryType type = fetch_message.InventoryType;
            List<Transaction> transactions = new List<Transaction>();

            int size = 0;

            foreach (SHA256Hash hash in fetch_message.GetHashList())
            {
                Item item = new Item(hash, type);

                Message msg = this.advance_service.GetMessage(item);
                if (msg == null)
                {
                    try
                    {
                        msg = this.net_delegate.GetData(hash, type);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error(
                            string.Format("Fetch item {0} failed. reason: {1}",
                                          item,
                                          hash,
                                          e.Message));
                        peer.Disconnect(ReasonCode.FetchFail);

                        return;
                    }
                }

                if (type == InventoryType.Block)
                {
                    BlockId block_id = ((BlockMessage)msg).Block.Id;
                    if (peer.BlockBothHave.Num < block_id.Num)
                    {
                        peer.BlockBothHave = block_id;
                    }

                    peer.SendMessage(msg);
                }
                else
                {
                    transactions.Add(((TransactionMessage)msg).Transaction.Instance);
                    size += ((TransactionMessage)msg).Transaction.Instance.CalculateSize();

                    if (size > MAX_SIZE)
                    {
                        peer.SendMessage(new TransactionsMessage(transactions));
                        transactions = new List<Transaction>();
                        size = 0;
                    }
                }
            }
            if (transactions.Count > 0)
            {
                peer.SendMessage(new TransactionsMessage(transactions));
            }
        }
        #endregion
    }
}
