using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Overlay.Server;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;

namespace Mineral.Core.Net
{
    public class MineralNetService
    {
        #region Field
        private static MineralNetService instance = null;

        private ChannelManager channel_manager;
        #endregion


        #region Property
        public static MineralNetService Instance
        {
            get { return instance ?? new MineralNetService(); }
        }
        #endregion


        #region Constructor
        private MineralNetService() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void OnMessage(PeerConnection peer, Messages.MineralMessage msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case MessageTypes.MsgType.SYNC_BLOCK_CHAIN:
                        SyncBlockChainMsgHandler.processMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.BLOCK_CHAIN_INVENTORY:
                        ChainInventoryMsgHandler.processMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.INVENTORY:
                        InventoryMsgHandler.processMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.FETCH_INV_DATA:
                        FetchInvDataMsgHandler.processMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.BLOCK:
                        BlockMsgHandler.processMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.TXS:
                        TransactionsMsgHandler.processMessage(peer, msg);
                        break;
                    default:
                        throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, msg.Type.ToString());
                }
            }
            catch (System.Exception e)
            {
                ProcessException(peer, msg, e);
            }
        }
        #endregion
    }
}
