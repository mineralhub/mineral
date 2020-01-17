using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Utils;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Utils;
using static Mineral.Utils.ScheduledExecutorService;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.Service
{
    public class AdvanceService
    {
        #region Field
        private ConcurrentDictionary<Item, long> inventory_fetch = new ConcurrentDictionary<Item, long>(Environment.ProcessorCount * 2, 50000);
        private ConcurrentDictionary<Item, long> inventory_spread = new ConcurrentDictionary<Item, long>(Environment.ProcessorCount * 2, 50000);

        private Cache<Message> block_cache = new Cache<Message>("advance_block").MaxCapacity(10).ExpireTime(TimeSpan.FromMinutes(1));
        private Cache<Message> transaction_cache = new Cache<Message>("advance_transaction").MaxCapacity(50000).ExpireTime(TimeSpan.FromHours(1));
        private Cache<long> inventory_fetch_cache = new Cache<long>("advance_fetch").MaxCapacity(100000).ExpireTime(TimeSpan.FromHours(1));

        private ScheduledExecutorHandle handle_spread = null;
        private ScheduledExecutorHandle handle_fetch = null;

        private MessageCount tx_count = new MessageCount();
        private int max_spread_size = 1000;
        #endregion


        #region Property
        public MessageCount TxCount
        {
            get { return this.tx_count; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ConsumerInventoryToSpread()
        {
            List<PeerConnection> peers = 
                Manager.Instance.NetDelegate.ActivePeers.Where(peer => !peer.IsNeedSyncFromPeer && !peer.IsNeedSyncUs).ToList();

            if (this.inventory_spread.IsEmpty || peers.IsNullOrEmpty())
            {
                return;
            }

            InventorySender sender = new InventorySender();
            
            foreach (var spread in this.inventory_spread)
            {
                foreach (PeerConnection peer in peers)
                {
                    if (peer.GetInventoryReceive(spread.Key) == null
                        && peer.GetInventorySpread(spread.Key) == null)
                    {

                        peer.AddInventorySpread(spread.Key, Helper.CurrentTimeMillis());
                        sender.Add(spread.Key, peer);
                    }
                    this.inventory_spread.TryRemove(spread.Key, out _);
                }
            }

            sender.SendInventory();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ConsumerInventoryToFetch()
        {
            List<PeerConnection> peers =
                Manager.Instance.NetDelegate.ActivePeers.Where(peer => peer.IsIdle).ToList();

            if (this.inventory_fetch.IsEmpty || peers.IsNullOrEmpty())
            {
                return;
            }

            InventorySender sender = new InventorySender();
            long now = Helper.CurrentTimeMillis();

            foreach (var fetch in this.inventory_fetch)
            {
                if (fetch.Value
                    < now - Parameter.NetParameters.MSG_CACHE_DURATION_IN_BLOCKS * Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL)
                {
                    Logger.Info(
                        string.Format("This obj is too late to fetch, type: {0} hash: {1}.",
                                      fetch.Key.Type,
                                      fetch.Key.Hash));

                    this.inventory_fetch.TryRemove(fetch.Key, out _);
                    this.inventory_fetch_cache.Remove(fetch.Key.ToString());
                    return;
                }

                peers.Where(peer => peer.GetInventoryReceive(fetch.Key) != null
                                    && sender.GetSize(peer) < Parameter.NetParameters.MAX_TRX_FETCH_PER_PEER)
                     .OrderBy(peer => sender.GetSize(peer))
                     .FirstOrDefault(peer =>
                     {
                         sender.Add(fetch.Key, peer);
                         peer.InventoryRequest.TryAdd(fetch.Key, now);
                         inventory_fetch.TryRemove(fetch.Key, out _);
                         return true;
                     });
            }

            sender.SendFetch();
        }
        #endregion


        #region External Method
        public void Init()
        {
            if (Args.Instance.IsFastForward)
            {
                return;
            }

            this.handle_spread = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    ConsumerInventoryToSpread();
                }
                catch (System.Exception e)
                {
                    Logger.Error("Spread thread error. " + e.Message);
                }
            }, 100, 30);

            this.handle_fetch = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    ConsumerInventoryToFetch();
                }
                catch (System.Exception e)
                {
                    Logger.Error("Fetch thread error." + e.Message);
                }
            }, 100, 30);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool AddInventory(Item item)
        {
            if (Args.Instance.IsFastForward
                && item.Type != InventoryType.Block)
            {
                return false;
            }

            if (this.inventory_fetch_cache.Get(item.ToString()) != null)
            {
                return false;
            }

            if (item.Type.Equals(InventoryType.Trx))
            {
                if (this.transaction_cache.Get(item.ToString()) != null)
                {
                    return false;
                }
            }
            else
            {
                if (this.block_cache.Get(item.ToString()) != null)
                {
                    return false;
                }
            }


            this.inventory_fetch_cache.Add(item.ToString(), Helper.CurrentTimeMillis());
            this.inventory_fetch.TryAdd(item, Helper.CurrentTimeMillis());

            if (item.Type == InventoryType.Block)
            {
                ConsumerInventoryToFetch();
            }

            return true;
        }

        public Message GetMessage(Item item)
        {
            if (item.Type == InventoryType.Trx)
            {
                return (Message)this.transaction_cache.Get(item.ToString());
            }
            else
            {
                return (Message)this.block_cache.Get(item.ToString());
            }
        }

        public void Broadcast(Message message)
        {
            if (Args.Instance.IsFastForward
                && !(message is BlockMessage))
            {
                return;
            }

            if (this.inventory_spread.Count > this.max_spread_size)
            {
                Logger.Warning(
                    string.Format("Drop message, type: {0}, ID: {1}.",
                                  message.Type,
                                  message.MessageId));
                return;
            }

            Item item = null;
            if (message is BlockMessage)
            {
                BlockMessage block_message = (BlockMessage)message;

                item = new Item(block_message.MessageId, InventoryType.Block);
                Logger.Info("Ready to broadcast block " + block_message.Block.Id.GetString());

                block_message.Block.Transactions.ForEach(tx =>
                {
                    var find = this.inventory_spread.FirstOrDefault(pair => pair.Key.Hash == tx.Id);
                    if (!find.Equals(default(KeyValuePair<Item, long>)))
                    {
                        this.inventory_spread.TryRemove(find.Key, out _);
                        this.transaction_cache.Add(new Item(find.Key.Hash, InventoryType.Trx).ToString(), new TransactionMessage(tx.Instance));
                    }
                });

                this.block_cache.Add(item.ToString(), message);
            }
            else if (message is TransactionMessage)
            {
                TransactionMessage tx_message = (TransactionMessage)message;
                item = new Item(tx_message.MessageId, InventoryType.Trx);

                this.tx_count.Add();
                this.transaction_cache.Add(item.ToString(), new TransactionMessage(((TransactionMessage)message).Transaction.Instance));
            }
            else
            {
                Logger.Error("Adv item is neither block nor trx, type : " + message.Type.ToString());
                return;
            }

            this.inventory_spread.TryAdd(item, Helper.CurrentTimeMillis());
            if (item.Type == InventoryType.Block)
            {
                ConsumerInventoryToSpread();
            }
        }

        public void OnDisconnect(PeerConnection peer)
        {
            if (!peer.InventoryRequest.IsEmpty)
            {
                foreach (Item item in peer.InventoryRequest.Keys)
                {
                    if (Manager.Instance.NetDelegate.ActivePeers.FirstOrDefault(p => p != peer && p.GetInventoryReceive(item) != null) != null)
                    {
                        this.inventory_fetch.TryAdd(item, Helper.CurrentTimeMillis());
                    }
                    else
                    {
                        this.inventory_fetch_cache.Remove(item.ToString());
                    }
                }
            }

            if (this.inventory_fetch.Count > 0)
            {
                ConsumerInventoryToFetch();
            }
        }

        public void Close()
        {
            this.handle_fetch.Shutdown();
            this.handle_spread.Shutdown();
        }
        #endregion
    }
}
