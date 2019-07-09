using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DequeNet;
using Mineral.Core.Config;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Core.Net.Service;
using Mineral.Utils;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Net.MessageHandler
{
    public class ChainInventoryMessageHandler : IMessageHandler
    {
        #region Field
        private MineralNetDelegate net_delegate = null;
        private SyncService sync_service = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Check(PeerConnection peer, ChainInventoryMessage message)
        {
            if (peer.SyncChainRequest.Equals(default(KeyValuePair<Deque<BlockId>, long>)))
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "not send syncBlockChainMsg");
            }

            List<BlockId> ids = message.Ids;
            if (ids.IsNullOrEmpty())
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "blockIds is empty");
            }

            if (ids.Count > Parameter.NodeParameters.SYNC_FETCH_BATCH_NUM + 1)
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "big blockIds size: " + ids.Count);
            }

            if (message.RemainNum != 0 && ids.Count < Parameter.NodeParameters.SYNC_FETCH_BATCH_NUM)
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "remain: " + message.RemainNum + ", blockIds size: " + ids.Count);
            }

            long num = ids[0].Num;
            foreach (BlockId id in message.Ids)
            {
                if (id.Num != num++)
                {
                    throw new P2pException(
                        P2pException.ErrorType.BAD_MESSAGE, "not continuous block");
                }
            }

            if (!peer.SyncChainRequest.Key.Contains(ids[0]))
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE,
                    "unlinked block, my head: "
                    + peer.SyncChainRequest.Key.LastOrDefault().GetString()
                    + ", peer: " + ids[0].GetString());
            }

            if (this.net_delegate.HeadBlockId.Num > 0)
            {
                long max_remain_time = Parameter.ChainParameters.CLOCK_MAX_DELAY
                                        + Helper.CurrentTimeMillis()
                                        - this.net_delegate.GetBlockTime(this.net_delegate.SolidBlockId);
                long max_future_num =
                    max_remain_time / Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL + this.net_delegate.SolidBlockId.Num;
                long last_num = ids[ids.Count - 1].Num;

                if (last_num + message.RemainNum > max_future_num)
                {
                    throw new P2pException(P2pException.ErrorType.BAD_MESSAGE, "lastNum: " + last_num + " + remainNum: "
                        + message.RemainNum + " > futureMaxNum: " + max_future_num);
                }
            }
        }
        #endregion


        #region External Method
        public void ProcessMessage(PeerConnection peer, MineralMessage message)
        {
            ChainInventoryMessage chain_inventory_message = (ChainInventoryMessage)message;
            Check(peer, chain_inventory_message);

            peer.IsNeedSyncPeer = true;
            peer.SyncChainRequest = default(KeyValuePair<DequeNet.Deque<Capsule.BlockCapsule.BlockId>, long>);

            Deque<BlockId> block_id = new Deque<BlockId>(chain_inventory_message.Ids);

            if (block_id.Count == 1 && this.net_delegate.ContainBlock(block_id.FirstOrDefault()))
            {
                peer.IsNeedSyncPeer = false;
                return;
            }

            while (!peer.SyncBlockFetch.IsEmpty)
            {
                if (peer.SyncBlockFetch.LastOrDefault().Equals(block_id.LastOrDefault()))
                {
                    break;
                }
                peer.SyncBlockFetch.TryPopRight(out _);
            }

            block_id.PopLeft();

            peer.RemainNum = chain_inventory_message.RemainNum;

            foreach (BlockId id in block_id)
            {
                peer.SyncBlockFetch.PushLeft(id);
            }

            lock (this.net_delegate.LockBlock)
            {
                while (!peer.SyncBlockFetch.IsEmpty
                    && this.net_delegate.ContainBlock(peer.SyncBlockFetch.First()))
                {
                    peer.SyncBlockFetch.TryPopLeft(out BlockId id);
                    peer.BlockBothHave = id;
                    Logger.Info(
                        string.Format("Block {0} from {1} is processed", id.GetString(), peer.Node.Host));
                }
            }

            if ((chain_inventory_message.RemainNum == 0 && !peer.SyncBlockFetch.IsEmpty)
                || (chain_inventory_message.RemainNum != 0&& peer.SyncBlockFetch.Count > Parameter.NodeParameters.SYNC_FETCH_BATCH_NUM))
            {
                this.sync_service.IsFetch = true;
            }
            else
            {
                this.sync_service.SyncNext(peer);
            }
        }
        #endregion
    }
}
