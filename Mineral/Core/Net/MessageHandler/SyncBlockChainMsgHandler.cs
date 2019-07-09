using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Common.Overlay.Messages;
using Mineral.Core.Config;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Utils;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Net.MessageHandler
{
    public class SyncBlockChainMessageHandler : IMessageHandler
    {
        #region Field
        private MineralNetDelegate net_delegate = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Check(PeerConnection peer, SyncBlockChainMessage message)
        {
            List<BlockId> ids = message.Ids;
            if (ids.IsNullOrEmpty())
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "SyncBlockChain blockIds is empty");
            }

            BlockId first_id = ids.First();
            if (!this.net_delegate.ContainBlockInMainChain(first_id))
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "No first block:" + first_id.GetString());
            }

            long head_num = this.net_delegate.HeadBlockId.Num;
            if (first_id.Num > head_num)
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE,
                    "First blockNum:" + first_id.Num + " gt my head BlockNum:" + head_num);
            }

            BlockId last_sync_id = peer.LastSyncBlockId;
            long last_num = ids[(ids.Count - 1)].Num;

            if (last_sync_id != null
                && last_sync_id.Num > last_num)
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE,
                    "lastSyncNum:" + last_sync_id.Num + " gt lastNum:" + last_num);
            }
        }

        private List<BlockId> GetLostBlockIds(List<BlockId> block_ids)
        {
            BlockId unfork_id = null;

            for (int i = block_ids.Count - 1; i >= 0; i--)
            {
                if (this.net_delegate.ContainBlockInMainChain(block_ids[i]))
                {
                    unfork_id = block_ids[i];
                    break;
                }
            }

            if (unfork_id == null)
            {
                throw new P2pException(
                    P2pException.ErrorType.SYNC_FAILED, "unForkId is null");
            }

            long len = Math.Min(this.net_delegate.HeadBlockId.Num, unfork_id.Num + Parameter.NodeParameters.SYNC_FETCH_BATCH_NUM);

            List<BlockId> ids = new List<BlockId>();
            for (long i = unfork_id.Num; i <= len; i++)
            {
                BlockId id = this.net_delegate.GetBlockIdByNum(i);
                ids.Add(id);
            }

            return ids;
        }
        #endregion


        #region External Method
        public void ProcessMessage(PeerConnection peer, Messages.MineralMessage message)
        {
            SyncBlockChainMessage sync_message = (SyncBlockChainMessage)message;
            Check(peer, sync_message);

            long remain_num = 0;
            List<BlockId> ids = GetLostBlockIds(sync_message.Ids);

            if (ids.Count == 1)
            {
                peer.IsNeedSyncUs = false;
            }
            else
            {
                peer.IsNeedSyncUs = true;
                remain_num = this.net_delegate.HeadBlockId.Num - ids.Last().Num;
            }

            peer.LastSyncBlockId = ids.Last();
            peer.RemainNum = remain_num;
            peer.SendMessage(new ChainInventoryMessage(ids, remain_num));
        }
        #endregion
    }
}
