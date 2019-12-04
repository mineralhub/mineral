using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Mineral.Common.Overlay.Client;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Core;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Net.Peer;
using Mineral.Utils;
using static Mineral.Utils.ScheduledExecutorService;

namespace Mineral.Common.Overlay.Server
{
    public class SyncPool
    {
        #region Field
        private BlockingCollection<PeerConnection> active_peers = new BlockingCollection<PeerConnection>();
        private int passive_peers_count = 0;
        private int active_peers_count = 0;

        private CacheItemPolicy node_handler_policy = new CacheItemPolicy();
        private MemoryCache node_handler = new MemoryCache("node_handler_cache");
        private double factor = Args.Instance.Node.ConnectFactor;
        private double factor_active = Args.Instance.Node.ActiveConnectFactor;
        private int max_active_nodes = Args.Instance.Node.MaxActiveNodes;
        private int max_active_peers_same_ip = Args.Instance.Node.MaxActiveNodeSameIP;

        private ScheduledExecutorHandle timer_pool = null;
        private ScheduledExecutorHandle timer_log = null;
        #endregion


        #region Property
        public List<PeerConnection> ActivePeers
        {
            get
            {
                List<PeerConnection> peers = new List<PeerConnection>();
                foreach (PeerConnection peer in this.active_peers)
                {
                    if (!peer.IsDisconnect)
                    {
                        peers.Add(peer);
                    }
                }

                return peers;
            }
        }

        public int PassivePeerCount
        {
            get { return this.passive_peers_count; }
        }

        public int ActivePeerCount
        {
            get { return this.active_peers_count; }
        }

        public bool IsCanConnect
        {
            get { return this.passive_peers_count < this.max_active_nodes * (1 - this.factor_active); }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void FillUp()
        {
            List<NodeHandler> connect_nodes = new List<NodeHandler>();
            HashSet<IPAddress> address_use = new HashSet<IPAddress>();
            HashSet<string> nodes_use = new HashSet<string>();

            foreach (var channel in Manager.Instance.ChannelManager.ActivePeer)
            {
                nodes_use.Add(channel.PeerId);
                address_use.Add(channel.Address);
            }

            foreach (var active in Manager.Instance.ChannelManager.ActiveNodes)
            {
                nodes_use.Add(active.Value.Id.ToHexString());
                if (!address_use.Contains(active.Key))
                {
                    connect_nodes.Add(Manager.Instance.NodeManager.GetNodeHandler(active.Value));
                }
            }

            int size = Math.Max((int)(this.max_active_nodes * this.factor) - this.active_peers.Count,
                                (int)(this.max_active_nodes * this.factor_active - this.active_peers_count));

            int lack_size = size - connect_nodes.Count;
            if (lack_size > 0)
            {
                nodes_use.Add(Manager.Instance.NodeManager.PublicHomeNode.Id.ToHexString());
                List<NodeHandler> new_nodes = Manager.Instance.NodeManager.GetNodes(NodeSelector, nodes_use, lack_size);
                connect_nodes.AddRange(new_nodes);
            }

            connect_nodes.ForEach(node =>
            {
                Manager.Instance.PeerClient.ConnectAsync(node, false);
                this.node_handler_policy.AbsoluteExpiration = DateTime.Now.AddSeconds(180);
                this.node_handler.Add(node.ToString(), Helper.CurrentTimeMillis(), this.node_handler_policy);
            });
        }

        private bool NodeSelector(NodeHandler handler, HashSet<string> nodes_use)
        {
            if (handler.Node.Host == Manager.Instance.NodeManager.PublicHomeNode.Host
                && handler.Node.Port == Manager.Instance.NodeManager.PublicHomeNode.Port)
            {
                return false;
            }

            if (nodes_use != null && nodes_use.Contains(handler.Node.Id.ToHexString()))
            {
                return false;
            }

            if (handler.NodeStatistics.Reputation >= NodeStatistics.REPUTATION_PREDEFINED)
            {
                return true;
            }

            IPAddress address = handler.SocketAddress.Address;
            if (Manager.Instance.ChannelManager.RecentlyDisconnected.Get(address.ToString()) != null)
            {
                return false;
            }

            if (Manager.Instance.ChannelManager.BadPeers.Get(address.ToString()) != null)
            {
                return false;
            }

            if (Manager.Instance.ChannelManager.GetConnectionNum(address) >= this.max_active_peers_same_ip)
            {
                return false;
            }

            if (this.node_handler.Get(handler.ToString()) != null)
            {
                return false;
            }

            if (handler.NodeStatistics.Reputation < 100)
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void LogActivePeers()
        {
            Logger.Info(string.Format("-------- active connect channel {0}", this.active_peers_count));
            Logger.Info(string.Format("-------- passive connect channel {0}", this.passive_peers_count));
            Logger.Info(string.Format("-------- all connect channel {0}", Manager.Instance.ChannelManager.ActivePeer.Count));

            foreach (Channel channel in Manager.Instance.ChannelManager.ActivePeer)
            {
                Logger.Info(channel.ToString());
            }

            StringBuilder sb = new StringBuilder("Peer stats:\n");
            sb.Append("Active peers\n");
            sb.Append("============\n");
            HashSet<Node> active = new HashSet<Node>();

            foreach (PeerConnection peer in this.active_peers)
            {
                sb.Append(peer.Log()).Append('\n');
                active.Add(peer.Node);
            }
            sb.Append("Other connected peers\n");
            sb.Append("============\n");
            foreach (Channel peer in Manager.Instance.ChannelManager.ActivePeer)
            {
                if (!active.Contains(peer.Node))
                {
                    sb.Append(peer.Node).Append('\n');
                }
            }

            Logger.Info(sb.ToString());
        }
        #endregion


        #region External Method
        public void Init()
        {
            this.timer_pool = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    FillUp();
                }
                catch (System.Exception e)
                {
                    Logger.Error("Exception in sync worker", e);
                }
            }, 30 * 1000, 3600);

            this.timer_log = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    LogActivePeers();
                }
                catch
                {
                }
            }, 30000, 10000);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnConnect(Channel channel)
        {
            PeerConnection peer = (PeerConnection)channel;
            if (!this.active_peers.Contains(peer))
            {
                if (!peer.IsActive)
                {
                    Interlocked.Increment(ref this.passive_peers_count);
                }
                else
                {
                    Interlocked.Increment(ref this.active_peers_count);
                }

                this.active_peers.Add(peer);
                this.active_peers.OrderBy(p => p.PeerStatistics.AverageLatency);
                peer.OnConnect();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnDisconnect(Channel channel)
        {
            PeerConnection peer = (PeerConnection)channel;
            if (this.active_peers.Contains(peer))
            {
                if (!peer.IsActive)
                {
                    Interlocked.Decrement(ref this.passive_peers_count);
                }
                else
                {
                    Interlocked.Decrement(ref this.active_peers_count);
                }

                this.active_peers.Remove(peer);
                peer.OnDisconnect();
            }
        }

        public void Close()
        {
            try
            {
                this.timer_pool.Shutdown();
                this.timer_log.Shutdown();
            }
            catch (Exception)
            {
                Logger.Warning("Problems shutting down executor");
            }
        }
        #endregion
    }
}
