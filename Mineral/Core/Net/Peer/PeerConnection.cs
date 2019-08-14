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
using Mineral.Core.Config;
using Mineral.Core.Net.Service;
using Mineral.Utils;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Net.Peer
{
    public class PeerConnection : Channel
    {
        #region Field
        private HelloMessage hello_message = null;

        private BlockId singup_error_id = null;
        private BlockId block_both_have = null;
        private BlockId last_sync_id = null;
        private Cache<long> inventory_receive = new Cache<long>().MaxCapacity(100000).ExpireTime(TimeSpan.FromHours(1));
        private Cache<long> inventory_spread = new Cache<long>().MaxCapacity(100000).ExpireTime(TimeSpan.FromHours(1));
        private Cache<long> sync_block_id = new Cache<long>().MaxCapacity(2 * Parameter.NodeParameters.SYNC_FETCH_BATCH_NUM);

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
            this.inventory_receive.Add(key.ToString(), value);
        }

        public void AddInventorySpread(Item key, long value)
        {
            this.inventory_spread.Add(key.ToString(), value);
        }

        public void AddSyncBlockId(SHA256Hash key, long value)
        {
            this.sync_block_id.Add(key.Hash.ToHexString(), value);
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
            if (this.hello_message.HeadBlockId.Num > Manager.Instance.NetDelegate.HeadBlockId.Num)
            {
                State = MineralState.SYNCING;
                Manager.Instance.SyncService.StartSync(this);
            }
            else
            {
                State = MineralState.SYNC_COMPLETED;
            }
        }

        public void OnDisconnect()
        {
            Manager.Instance.SyncService.OnDisconnect(this);
            Manager.Instance.AdvanceService.OnDisconnect(this);

            this.inventory_receive = new Cache<long>().MaxCapacity(100000).ExpireTime(TimeSpan.FromHours(1));
            this.inventory_spread = new Cache<long>().MaxCapacity(100000).ExpireTime(TimeSpan.FromHours(1));
            this.inventory_request.Clear();

            this.sync_block_id = new Cache<long>().MaxCapacity(2 * Parameter.NodeParameters.SYNC_FETCH_BATCH_NUM);
            this.sync_block_fetch.Clear();
            this.sync_block_request.Clear();
            this.sync_block_process.Clear();
        }

        public string Log()
        {
            long now = Helper.CurrentTimeMillis();

            this.sync_block_fetch.TryPeekLeft(out BlockId id);

            return string.Format(
                "Peer {0} : [ {1:18}, ping {2:6} ms]-----------\n"
                    + "connect time : {3}s\n"
                    + "last know block num : {4}\n"
                    + "needSyncFromPeer : {5}\n"
                    + "needSyncFromUs : {6}\n"
                    + "syncToFetchSize : {7}\n"
                    + "syncToFetchSizePeekNum : {8}\n"
                    + "syncBlockRequestedSize : {9}\n"
                    + "remainNum : {10}\n"
                    + "syncChainRequested : {11}\n"
                    + "blockInProcess : {12}\n",
                base.Node.Host + ":" + base.Node.Port,
                base.Node.Id.ToHexString(),
                (int)base.PeerStatistics.AverageLatency,
                (now - base.StartTime) / 1000,
                this.block_both_have.Num,
                IsNeedSyncPeer,
                IsNeedSyncUs,
                this.sync_block_fetch.Count,
                this.sync_block_fetch.Count > 0 ? id.Num : -1,
                this.sync_block_request.Count,
                this.remain_num,
                this.sync_chain_request.Equals(default(KeyValuePair<Deque<BlockId>, long>)) ?
                            0 : (now - this.sync_chain_request.Value) / 1000,
                this.sync_block_process.Count + this.node_statistics.ToString() + "\n");
        }
        #endregion
    }
}
