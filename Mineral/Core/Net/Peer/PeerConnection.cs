using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using DequeNet;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Overlay.Server;
using Mineral.Common.Utils;
using Mineral.Core.Net.Service;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Net.Peer
{
    public class PeerConnection : Channel
    {
        #region Field
        private MineralNetDelegate net_delegate = null;
        private SyncService sync_service = null;
        private AdvanceService advance_service = null;
        private HelloMessage hello_message = null;

        private BlockId singup_error_id = null;
        private BlockId block_both_have = null;
        private BlockId last_sync_id = null;
        private MemoryCache inventory_receive = MemoryCache.Default;
        private MemoryCache inventory_spread = MemoryCache.Default;
        private MemoryCache sync_block_id = MemoryCache.Default;

        private KeyValuePair<Deque<BlockId>, long> sync_chain_request = default(KeyValuePair<Deque<BlockId>, long>);
        private HashSet<BlockId> sync_block_process = new HashSet<BlockId>();
        private ConcurrentDeque<BlockId> sync_block_fetch = new ConcurrentDeque<BlockId>();
        private ConcurrentDictionary<BlockId, long> sync_block_request = new ConcurrentDictionary<BlockId, long>();
        private ConcurrentDictionary<Item, long> inventory_request = new ConcurrentDictionary<Item, long>();

        private int inventory_cache_size = 100_000;
        private long block_both_have_timestamp = Helper.CurrentTimeMillis();
        private long remain_num = 0;
        private bool need_sync_peer = false;
        private bool ndeed_sync_us = false;
        #endregion


        #region Property
        public HelloMessage HelloMessage
        {
            get { return this.hello_message; }
            set { this.hello_message = value; }
        }

        public BlockId SignupErrorBlockId
        {
            get { return this.singup_error_id; }
            set { this.singup_error_id = value; }
        }

        public BlockId BlockBothHave
        {
            get { return this.block_both_have; }
            set
            {
                this.block_both_have = value;
                this.block_both_have_timestamp = Helper.CurrentTimeMillis();
            }
        }

        public BlockId LastSyncBlockId
        {
            get { return this.last_sync_id; }
            set { this.last_sync_id = value; }
        }

        //public MemoryCache InventoryReceive
        //{
        //    get { return this.inventory_receive; }
        //    set { this.inventory_receive = value; }
        //}

        //public MemoryCache InventorySpread
        //{
        //    get { return this.inventory_spread; }
        //    set { this.inventory_spread = value; }
        //}

        //public MemoryCache SyncBlockId
        //{
        //    get { return this.sync_block_id; }
        //    set { this.sync_block_id = value; }
        //}

        public KeyValuePair<Deque<BlockId>, long> SyncChainRequest
        {
            get { return this.sync_chain_request; }
            set { this.sync_chain_request = value; }
        }

        public HashSet<BlockId> SyncBlockProcess
        {
            get { return this.sync_block_process; }
            set { this.sync_block_process = value; }
        }

        public ConcurrentDeque<BlockId> SyncBlockFetch
        {
            get { return this.sync_block_fetch; }
            set { this.sync_block_fetch = value; }
        }

        public ConcurrentDictionary<BlockId, long> SyncBlockRequest
        {
            get { return this.sync_block_request; }
            set { this.sync_block_request = value; }
        }

        public ConcurrentDictionary<Item, long> InventoryRequest
        {
            get { return this.inventory_request; }
            set { this.inventory_request = value; }
        }

        public long BlockBothHaveTimestamp
        {
            get { return this.block_both_have_timestamp; }
        }

        public long RemainNum
        {
            get { return this.remain_num; }
            set { this.remain_num = value; }
        }

        public bool IsNeedSyncPeer
        {
            get { return this.need_sync_peer; }
            set { this.need_sync_peer = value; }
        }

        public bool IsNeedSyncUs
        {
            get { return this.ndeed_sync_us; }
            set { this.ndeed_sync_us = value; }
        }

        public bool IsIdle
        {
            get
            {
                return this.inventory_request.IsEmpty
                    && this.sync_block_request.IsEmpty
                    && this.sync_chain_request.Equals(default(KeyValuePair<BlockingCollection<BlockId>, long>));
            }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void AddInventoryReceive(Item key, long value)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.AddHours(1);

            this.inventory_receive.Add(key.ToString(), value, policy);
        }

        public void AddInventorySpread(Item key, long value)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.AddHours(1);

            this.inventory_spread.Add(key.ToString(), value, policy);
        }

        public void AddSyncBlockId(SHA256Hash key, long value)
        {
            CacheItemPolicy policy = new CacheItemPolicy();

            this.sync_block_id.Add(key.Hash.ToHexString(), value, policy);
        }

        public object GetInventoryReceive(Item key)
        {
            return this.inventory_receive.Get(key.ToString());
        }

        public object GetInventorySpread(Item key)
        {
            return this.inventory_spread.Get(key.ToString());
        }

        public object GetSyncBlockId(SHA256Hash key)
        {
            return this.sync_block_id.Get(key.ToString());
        }

        public void RemoveInventoryReceive(Item key)
        {
            this.inventory_receive.Remove(key.ToString());
        }

        public void RemoveInventorySpread(Item key)
        {
            this.inventory_spread.Remove(key.ToString());
        }

        public void RemoveSyncBlockId(SHA256Hash key)
        {
            this.sync_block_id.Remove(key.ToString());
        }

        public void SendMessage(Message message)
        {
            this.message_queue.SendMessage(message);
        }

        public void OnConnect()
        {
            if (this.hello_message.HeadBlockId.Num > this.net_delegate.getHeadBlockId().getNum())
            {
                setTronState(TronState.SYNCING);
                syncService.startSync(this);
            }
            else
            {
                setTronState(TronState.SYNC_COMPLETED);
            }
        }

        public void onDisconnect()
        {
            this.syncService.onDisconnect(this);
            this.advService.onDisconnect(this);
            this.advInvReceive.cleanUp();
            this.advInvSpread.cleanUp();
            this.advInvRequest.clear();
            this.syncBlockIdCache.cleanUp();
            this.syncBlockToFetch.clear();
            this.syncBlockRequested.clear();
            this.syncBlockInProcess.clear();
            this.syncBlockInProcess.clear();
        }

        public string Log()
        {
            long now = System.currentTimeMillis();
 
            return String.format(
                "Peer %s: [ %18s, ping %6s ms]-----------\n"
                    + "connect time: %ds\n"
                    + "last know block num: %s\n"
                    + "needSyncFromPeer:%b\n"
                    + "needSyncFromUs:%b\n"
                    + "syncToFetchSize:%d\n"
                    + "syncToFetchSizePeekNum:%d\n"
                    + "syncBlockRequestedSize:%d\n"
                    + "remainNum:%d\n"
                    + "syncChainRequested:%d\n"
                    + "blockInProcess:%d\n",
                this.getNode().getHost() + ":" + this.getNode().getPort(),
                this.getNode().getHexIdShort(),
                (int)this.getPeerStats().getAvgLatency(),
                (now - super.getStartTime()) / 1000,
                blockBothHave.getNum(),
                isNeedSyncFromPeer(),
                isNeedSyncFromUs(),
                syncBlockToFetch.size(),
                syncBlockToFetch.size() > 0 ? syncBlockToFetch.peek().getNum() : -1,
                syncBlockRequested.size(),
                remainNum,
                syncChainRequested == null ? 0 : (now - syncChainRequested.getValue()) / 1000,
                syncBlockInProcess.size())
                + nodeStatistics.toString() + "\n";
        }
        #endregion
    }
}
