using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using DequeNet;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Utils;
using static Mineral.Common.Overlay.Server.Channel;
using static Mineral.Core.Capsule.BlockCapsule;
using static Mineral.Utils.ScheduledExecutorService;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.Service
{
    public class SyncService
    {
        #region Field
        private MineralNetDelegate net_delegate = null;
        private ConcurrentDictionary<BlockMessage, PeerConnection> block_wait_process = new ConcurrentDictionary<BlockMessage, PeerConnection>();
        private ConcurrentDictionary<BlockMessage, PeerConnection> block_just_receive = new ConcurrentDictionary<BlockMessage, PeerConnection>();
        private MemoryCache request_ids = MemoryCache.Default;

        private ScheduledExecutorHandle fetch_handle = null;
        private ScheduledExecutorHandle block_handle = null;

        private volatile bool is_handle = false;
        private volatile bool is_fetch = false;
        #endregion


        #region Property
        public bool IsFetch
        {
            get { return this.is_fetch; }
            set { this.is_fetch = value; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void AddRequestBlockIds(BlockId key, long value)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.AddHours(1);

            this.request_ids.Add(key.ToString(), value, policy);
        }

        private void StartFetchSyncBlock()
        {
            Dictionary<PeerConnection, List<BlockId>> send = new Dictionary<PeerConnection, List<BlockId>>();

            foreach  (PeerConnection peer in this.net_delegate.ActivePeers.Where(peer => peer.IsNeedSyncPeer && peer.IsIdle))
            {
                if (!send.ContainsKey(peer))
                {
                    send.Add(peer, new List<BlockId>());
                }

                foreach (BlockId id in peer.SyncBlockFetch)
                {
                    if (this.request_ids.Get(id.ToString()) == null)
                    {
                        AddRequestBlockIds(id, Helper.CurrentTimeMillis());
                        peer.SyncBlockRequest.TryAdd(id, Helper.CurrentTimeMillis());
                        send[peer].Add(id);
                        if (send[peer].Count >= Parameter.NetParameters.MAX_BLOCK_FETCH_PER_PEER)
                        {
                            break;
                        }
                    }
                }
            }

            foreach (KeyValuePair<PeerConnection, List<BlockId>> pair in send)
            {
                if (pair.Value.IsNotNullOrEmpty())
                {
                    pair.Key.SendMessage(new FetchInventoryDataMessage(new List<SHA256Hash>(pair.Value), InventoryType.Block));
                }
            }
        }

        private void Invalid(BlockId id)
        {
            this.request_ids.Remove(id.ToString());
            this.is_fetch = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void HandleSyncBlock()
        {
            lock (this.block_just_receive)
            {
                foreach (var received in this.block_just_receive)
                {
                    this.block_wait_process.TryAdd(received.Key, received.Value);
                }
                this.block_just_receive.Clear();
            }

            bool is_processed = false;
            while (is_processed)
            {
                is_processed = false;

                foreach (KeyValuePair<BlockMessage, PeerConnection> process in this.block_wait_process)
                {
                    BlockMessage message = process.Key;
                    PeerConnection peer = process.Value;

                    if (peer.IsDisconnect)
                    {
                        this.block_wait_process.TryRemove(message, out _);
                        Invalid(message.Block.Id);
                        return;
                    }

                    bool is_found = false;
                    var peers = this.net_delegate.ActivePeers.Where(p => message.Block.Id.Equals(p.SyncBlockFetch.FirstOrDefault()));

                    foreach (PeerConnection p in peers)
                    {
                        peer.SyncBlockFetch.TryPopLeft(out _);
                        peer.SyncBlockProcess.Add(message.Block.Id);
                        is_found = true;
                    }

                    if (is_found)
                    {
                        this.block_wait_process.TryRemove(message, out _);
                        is_processed = true;
                        ProcessSyncBlock(message.Block);
                    }
                }
            }
        }

        private void ProcessSyncBlock(BlockCapsule block)
        {
            System.Exception exception = null;

            try
            {
                this.net_delegate.ProcessBlock(block);
            }
            catch (System.Exception e)
            {
                Logger.Error(
                    string.Format("Process sync block {0} failed.", block.Id.GetString()));
                exception = e;
            }

            foreach (PeerConnection peer in this.net_delegate.ActivePeers)
            {
                if (peer.SyncBlockProcess.Remove(block.Id))
                {
                    if (exception == null)
                    {
                        peer.BlockBothHave = block.Id;
                        if (peer.SyncBlockFetch.IsEmpty)
                        {
                            SyncNext(peer);
                        }
                    }
                    else
                    {
                        peer.Disconnect(Protocol.ReasonCode.BadBlock);
                    }
                }
            }
        }

        private List<BlockId> GetBlockChainSummary(PeerConnection peer)
        {
            BlockId begin_id = peer.BlockBothHave;
            List<BlockId> ids = new List<BlockId>(peer.SyncBlockFetch);
            List<BlockId> forks = new List<BlockId>();
            List<BlockId> summary = new List<BlockId>();

            long sync_begin = this.net_delegate.SyncBeginNumber;
            long low = sync_begin < 0 ? 0 : sync_begin;
            long hight_no_fork = 0; ;
            long high = 0;

            if (begin_id.Num == 0)
            {
                hight_no_fork = high = this.net_delegate.HeadBlockId.Num;
            }
            else
            {
                if (this.net_delegate.ContainBlockInMainChain(begin_id))
                {
                    hight_no_fork = high = begin_id.Num;
                }
                else
                {
                    forks = this.net_delegate.GetBlockChainHashesOnFork(begin_id);
                    if (forks.IsNullOrEmpty())
                    {
                        throw new P2pException(
                            P2pException.ErrorType.SYNC_FAILED, "can't find blockId : " + begin_id.GetString());
                    }

                    hight_no_fork = forks.LastOrDefault().Num;
                    forks.RemoveAt(forks.Count);
                    forks.Reverse();
                    high = hight_no_fork + forks.Count;
                }
            }

            if (low > hight_no_fork)
            {
                throw new P2pException(
                    P2pException.ErrorType.SYNC_FAILED, "low: " + low + " gt highNoFork: " + hight_no_fork);
            }

            long real_high = high + ids.Count;

            Logger.Info(
                string.Format("Get block chain summary, low: {0}, highNoFork: {1}, high: {2}, realHigh: {3}",
                              low,
                              hight_no_fork,
                              high,
                              real_high));

            while (low <= real_high)
            {
                if (low <= hight_no_fork)
                {
                    summary.Add(this.net_delegate.GetBlockIdByNum(low));
                }
                else if (low <= high)
                {
                    summary.Add(forks[(int)(low - hight_no_fork - 1)]);
                }
                else
                {
                    summary.Add(ids[(int)(low - high - 1)]);
                }
                low += (real_high - low + 2) / 2;
            }

            return summary;
        }
        #endregion


        #region External Method
        public void Init()
        {
            this.fetch_handle = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    if (this.is_fetch)
                    {
                        this.is_fetch = false;
                        StartFetchSyncBlock();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Error("Fetch sync block error.");
                }
            }, 10 * 1000, 1 * 1000);

            this.block_handle = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    if (this.is_handle)
                    {
                        this.is_handle = false;
                        HandleSyncBlock();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Error("Handle sync block error.");
                }

            }, 10 * 1000, 1 * 1000);
        }

        public void StartSync(PeerConnection peer)
        {
            peer.State = MineralState.SYNCING;
            peer.IsNeedSyncPeer = true;
            peer.SyncBlockFetch.Clear();
            peer.RemainNum = 0;
            peer.BlockBothHave = this.net_delegate.GenesisBlockId;
            SyncNext(peer);
        }

        public void SyncNext(PeerConnection peer)
        {
            try
            {
                if (!peer.SyncChainRequest.Equals(default(KeyValuePair<BlockingCollection<BlockId>, long>)))
                {
                    Logger.Warning(
                        string.Format("Peer {0} is in sync.",
                                      peer.Node.Host));
                    return;
                }

                List<BlockId> chain_summary = GetBlockChainSummary(peer);
                peer.SyncChainRequest = new KeyValuePair<Deque<BlockId>, long>(new Deque<BlockId>(chain_summary), Helper.CurrentTimeMillis());
                peer.SendMessage(new SyncBlockChainMessage(chain_summary));
            }
            catch (System.Exception e)
            {
                Logger.Error(
                    string.Format("Peer {0} sync failed, reason: {1}", peer.Address, e.Message));

                peer.Disconnect(Protocol.ReasonCode.SyncFail);
            }
        }

        public void OnDisconnect(PeerConnection peer)
        {
            if (!peer.SyncBlockRequest.IsEmpty)
            {
                foreach (BlockId id in peer.SyncBlockRequest.Keys)
                {
                    Invalid(id);
                }
            }
        }

        public void Close()
        {
            this.fetch_handle.Shutdown();
            this.block_handle.Shutdown();
        }
        #endregion
    }
}
