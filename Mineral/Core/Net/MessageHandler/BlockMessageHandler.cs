using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Core.Net.Service;
using static Mineral.Core.Capsule.BlockCapsule;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.MessageHandler
{
    public class BlockMessageHandler : IMessageHandler
    {
        #region Field
        private int max_block_size = Parameter.ChainParameters.BLOCK_SIZE + 1000;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Check(PeerConnection peer, BlockMessage msg)
        {
            Item item = new Item(msg.Block.Id, InventoryType.Block);

            if (!peer.SyncBlockRequest.ContainsKey(msg.Block.Id) && !peer.InventoryRequest.ContainsKey(item))
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "no request");
            }

            if (msg.Block.Instance.CalculateSize() > this.max_block_size)
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "block size over limit");
            }

            long gap = msg.Block.Timestamp - Helper.CurrentTimeMillis();
            if (gap >= Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL)
            {
                throw new P2pException(
                    P2pException.ErrorType.BAD_MESSAGE, "block time error");
            }
        }

        private void ProcessBlock(PeerConnection peer, BlockCapsule block)
        {
            BlockId block_id = block.Id;
            if (!Manager.Instance.NetDelegate.ContainBlock(block.ParentId))
            {
                Logger.Warning(
                    string.Format("Get unlink block {0} from {1}, head is {2}.",
                                  block.Id.GetString(),
                                  peer.Address.ToString(),
                                  Manager.Instance.NetDelegate.HeadBlockId.GetString()));

                Manager.Instance.SyncService.StartSync(peer);
                return;
            }

            if (Args.Instance.IsFastForward && Manager.Instance.NetDelegate.ValidBlock(block))
            {
                Manager.Instance.AdvanceService.Broadcast(new BlockMessage(block));
            }

            Manager.Instance.NetDelegate.ProcessBlock(block);
            Manager.Instance.WitnessBlockService.ValidWitnessProductTwoBlock(block);
            Manager.Instance.NetDelegate.ActivePeers.ForEach(p =>
            {
                if (p.GetInventoryReceive(new Item(block.Id, InventoryType.Block)) != null)
                {
                    p.BlockBothHave = block.Id;
                }
            });

            if (!Args.Instance.IsFastForward)
            {
                Manager.Instance.AdvanceService.Broadcast(new BlockMessage(block));
            }
        }
        #endregion


        #region External Method
        public void ProcessMessage(PeerConnection peer, MineralMessage message)
        {
            BlockMessage block_message = (BlockMessage)message;
            Check(peer, block_message);

            BlockId block_id = block_message.Block.Id;
            Item item = new Item(block_id, InventoryType.Block);

            if (peer.SyncBlockRequest.ContainsKey(block_id))
            {
                peer.RemoveSyncBlockId(block_id);
                Manager.Instance.SyncService.ProcessBlock(peer, block_message);
            }
            else
            {
                 peer.InventoryRequest.TryGetValue(item, out long ms);

                Logger.Info(
                    string.Format("Receive block {0} from {1}, cost {2}ms",
                                  block_id.GetString(),
                                  peer.Address,
                                  Helper.CurrentTimeMillis() - ms));

                peer.InventoryRequest.TryRemove(item, out _);
                ProcessBlock(peer, block_message.Block);
            }
        }
        #endregion
    }
}
