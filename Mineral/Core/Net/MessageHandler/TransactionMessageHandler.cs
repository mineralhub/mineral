using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Core.Net.Service;
using Mineral.Utils;
using Protocol;
using static Mineral.Utils.ScheduledExecutorService;
using static Protocol.Inventory.Types;
using static Protocol.Transaction.Types.Contract.Types;

namespace Mineral.Core.Net.MessageHandler
{
    public class TransactionMessageHandler : IMessageHandler
    {
        public class TxEvent
        {
            private PeerConnection peer = null;
            private TransactionMessage message = null;
            private long time;

            public PeerConnection Peer => this.peer;
            public TransactionMessage Message => this.message;
            public long Time => this.time;

            public TxEvent(PeerConnection peer, TransactionMessage message)
            {
                this.peer = peer;
                this.message = message;
                this.time = Helper.CurrentTimeMillis();
            }
        }

        #region Field
        private static readonly int MAX_TX_SIZE = 50000;
        private static readonly int MAX_SMART_CONTRACT_SUBMIT_SIZE = 100;

        private ConcurrentQueue<TxEvent> contract_queue = new ConcurrentQueue<TxEvent>();
        private ConcurrentQueue<TxEvent> wait_queue = new ConcurrentQueue<TxEvent>();

        private CountdownEvent cde = new CountdownEvent(0);
        private ScheduledExecutorHandle handle_contract = null;
        #endregion


        #region Property
        public bool IsBusy
        {
            get { return this.wait_queue.Count + contract_queue.Count > MAX_TX_SIZE; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Check(PeerConnection peer, TransactionsMessage message)
        {
            foreach (Transaction tx in message.Transactions.Transactions_)
            {
                Item item = new Item(new TransactionMessage(tx).MessageId, InventoryType.Trx);
                if (!peer.InventoryRequest.ContainsKey(item))
                {
                    throw new P2pException(P2pException.ErrorType.BAD_MESSAGE,
                        "tx: " + message.MessageId + " without request.");
                }
                peer.InventoryRequest.TryRemove(item, out _);
            }
        }

        private void HandleSmartContract()
        {
            this.handle_contract = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    while (wait_queue.Count < MAX_SMART_CONTRACT_SUBMIT_SIZE)
                    {
                        if (this.contract_queue.TryDequeue(out TxEvent tx_event))
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(HandleTransaction), new object[] { tx_event.Peer, tx_event.Message });
                        }
                    }
                }
                catch (System.Exception)
                {
                    Logger.Error("Handle smart contract exception.");
                }
            }, 1000, 20);
        }

        private static void HandleTransaction(object state)
        {
            using (Profiler.Measure("Handle Transaction"))
            {
                Profiler.PushFrame("Step-1");
                object[] parameter = state as object[];
                PeerConnection peer = (PeerConnection)parameter[0];
                TransactionMessage message = (TransactionMessage)parameter[1];

                if (peer.IsDisconnect)
                {
                    Logger.Warning(
                        string.Format("Drop tx {0} from {1}, peer is disconnect.",
                                      message.MessageId,
                                      peer.Address));

                    return;
                }

                Profiler.PushFrame("Step-2");
                if (Manager.Instance.AdvanceService.GetMessage(new Item(message.MessageId, InventoryType.Trx)) != null)
                {
                    return;
                }

                try
                {
                    Profiler.NextFrame("Step-3");
                    Manager.Instance.NetDelegate.PushTransaction(message.Transaction);
                    Profiler.NextFrame("Step-4");
                    Manager.Instance.AdvanceService.Broadcast(message);
                }
                catch (P2pException e)
                {
                    string reason = string.Format("Tx {0} from peer {1} process failed. type: {2}, reason: {3}",
                                                  message.MessageId.ToString(),
                                                  peer.Address.ToString(),
                                                  e.Type.ToString(),
                                                  e.Message.ToString());


                    Logger.Warning(reason);

                    if (e.Type == P2pException.ErrorType.BAD_TRX)
                    {
                        peer.Disconnect(ReasonCode.BadTx, reason);
                    }
                }
                catch
                {
                    Logger.Error(
                        string.Format("Tx {0} from peer {1} process failed.",
                                      message.MessageId.ToString(),
                                      peer.Address.ToString()));
                }

                Profiler.PopFrame();
            }
        }
        #endregion


        #region External Method
        public void Init()
        {
            HandleSmartContract();
        }

        public void ProcessMessage(PeerConnection peer, MineralMessage message)
        {
            using (Profiler.Measure("ProcessMessage : TX"))
            {
                Profiler.PushFrame("step-1");
                TransactionsMessage tx_message = (TransactionsMessage)message;
                Check(peer, tx_message);
                Profiler.NextFrame(string.Format("step-2, Txs : [{0}]", tx_message.Transactions.Transactions_.Count));

                foreach (Transaction tx in tx_message.Transactions.Transactions_)
                {
                    ContractType type = tx.RawData.Contract[0].Type;

                    if (type == ContractType.TriggerSmartContract || type == ContractType.CreateSmartContract)
                    {
                        this.contract_queue.Enqueue(new TxEvent(peer, new TransactionMessage(tx)));
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(HandleTransaction), new object[] { peer, new TransactionMessage(tx) });
                    }
                }
                Profiler.PopFrame();
            }
        }

        public void Close()
        {
            this.handle_contract.Shutdown();
        }
        #endregion
    }
}
