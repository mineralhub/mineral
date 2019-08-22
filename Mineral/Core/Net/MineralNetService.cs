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
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        public void OnMessage(PeerConnection peer, Messages.MineralMessage msg)
        {
            try
            {
                Logger.Refactoring(
                    string.Format("OnMessage : {0}", msg.Type.ToString()));

                switch (msg.Type)
                {
                    case MessageTypes.MsgType.SYNC_BLOCK_CHAIN:
                        {
                            Manager.Instance.SyncBlockHandler.ProcessMessage(peer, msg);
                        }
                        break;
                    case MessageTypes.MsgType.BLOCK_CHAIN_INVENTORY:
                        {
                            Manager.Instance.ChainInventoryHandler.ProcessMessage(peer, msg);
                        }
                        break;
                    case MessageTypes.MsgType.INVENTORY:
                        {
                            Manager.Instance.InventoryHandler.ProcessMessage(peer, msg);
                        }
                        break;
                    case MessageTypes.MsgType.FETCH_INV_DATA:
                        {
                            Manager.Instance.FetchInventoryHandler.ProcessMessage(peer, msg);
                        }
                        break;
                    case MessageTypes.MsgType.BLOCK:
                        {
                            Manager.Instance.BlockHandler.ProcessMessage(peer, msg);
                        }
                        break;
                    case MessageTypes.MsgType.TXS:
                        {
                            Manager.Instance.TransactionHandler.ProcessMessage(peer, msg);
                        }
                        break;
                    default:
                        {
                            throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, msg.Type.ToString());
                        }
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
            Manager.Instance.ChannelManager.Init();
            Manager.Instance.AdvanceService.Init();
            Manager.Instance.SyncService.Init();
            Manager.Instance.PeerStatusCheck.Init();
            Manager.Instance.TransactionHandler.Init();

            Logger.Info("NetService start successfully.");
        }

        public void Close()
        {
            Manager.Instance.ChannelManager.Close();
            Manager.Instance.AdvanceService.Close();
            Manager.Instance.SyncService.Close();
            Manager.Instance.PeerStatusCheck.Close();
            Manager.Instance.TransactionHandler.Close();

            Logger.Info("NetService closed successfully.");
        }

        public void Broadcast(Message message)
        {
            Manager.Instance.AdvanceService.Broadcast(message);
        }
        #endregion
    }
}
