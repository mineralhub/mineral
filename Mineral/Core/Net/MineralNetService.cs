using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Overlay.Server;
using Mineral.Core.Exception;
using Mineral.Core.Net.MessageHandler;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Core.Net.Service;
using Protocol;

namespace Mineral.Core.Net
{
    public class MineralNetService
    {
        #region Field
        private static MineralNetService instance = null;

        private ChannelManager channel_manager = null;
        private AdvanceService advance_service = null;
        private SyncService sync_service = null;
        private PeerStatusCheck peer_status = null;

        private SyncBlockChainMessageHandler handler_sync_block = null;
        private ChainInventoryMessageHandler handler_chain_inventory = null;
        private InventoryMessageHandler handler_inventory = null;
        private FetchInventoryDataMessageHandler handler_fetch_inventory = null;
        private BlockMessageHandler handler_block = null;
        private TransactionMessageHandler handler_transaction = null;
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
        public void OnMessage(PeerConnection peer, Messages.MineralMessage msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case MessageTypes.MsgType.SYNC_BLOCK_CHAIN:
                        this.handler_sync_block.ProcessMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.BLOCK_CHAIN_INVENTORY:
                        this.handler_chain_inventory.ProcessMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.INVENTORY:
                        this.handler_inventory.ProcessMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.FETCH_INV_DATA:
                        this.handler_fetch_inventory.ProcessMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.BLOCK:
                        this.handler_block.ProcessMessage(peer, msg);
                        break;
                    case MessageTypes.MsgType.TXS:
                        this.handler_transaction.ProcessMessage(peer, msg);
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

        private void ProcessException(PeerConnection peer, Messages.MineralMessage message, System.Exception exception)
        {
            ReasonCode code = ReasonCode.Unknown;

            if (exception is P2pException) {
                P2pException.ErrorType type = ((P2pException)exception).Type;
                switch (type)
                {
                    case P2pException.ErrorType.BAD_TRX:
                        code = ReasonCode.BadTx;
                        break;
                    case P2pException.ErrorType.BAD_BLOCK:
                        code = ReasonCode.BadBlock;
                        break;
                    case P2pException.ErrorType.NO_SUCH_MESSAGE:
                    case P2pException.ErrorType.MESSAGE_WITH_WRONG_LENGTH:
                    case P2pException.ErrorType.BAD_MESSAGE:
                        code = ReasonCode.BadProtocol;
                        break;
                    case P2pException.ErrorType.SYNC_FAILED:
                        code = ReasonCode.SyncFail;
                        break;
                    case P2pException.ErrorType.UNLINK_BLOCK:
                        code = ReasonCode.Unlinkable;
                        break;
                    default:
                        code = ReasonCode.Unknown;
                        break;
                }

                Logger.Error(
                    string.Format("Message from {0} process failed, {1} \n type: {2}, detail: {3}.",
                                  peer.Address, message, type, exception.Message));
            }
            else
            {
                code = ReasonCode.Unknown;
                Logger.Error(
                    string.Format("Message from {0} process failed, {1}",
                                  peer.Address,
                                  message));
            }

            peer.Disconnect(code);
        }
        #endregion


        #region External Method
        public void Start()
        {
            this.channel_manager.Init();
            this.advance_service.Init();
            this.sync_service.Init();
            this.peer_status.Init();
            this.handler_transaction.Init();

            Logger.Info("TronNetService start successfully.");
        }

        public void Close()
        {
            this.channel_manager.Close();
            this.advance_service.Close();
            this.sync_service.Close();
            this.peer_status.Close();
            this.handler_transaction.Close();

            Logger.Info("TronNetService closed successfully.");
        }

        public void Broadcast(Message message)
        {
            this.advance_service.Broadcast(message);
        }
        #endregion
    }
}
