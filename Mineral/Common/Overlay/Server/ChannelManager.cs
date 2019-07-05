using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Mineral.Common.Overlay.Client;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Overlay.Server
{
    public class ChannelManager
    {
        #region Field
        private static ChannelManager instance = null;

        private PeerServer peer_server = new PeerServer();
        private PeerClient peer_client = new PeerClient();
        private SyncPool sync_pool = SyncPool.Instance;

        private CacheItemPolicy bad_peers_policy = new CacheItemPolicy();
        private CacheItemPolicy recently_disconnected_policy = new CacheItemPolicy();
        private MemoryCache bad_peers = MemoryCache.Default;
        private MemoryCache recently_disconnected = MemoryCache.Default;

        private ConcurrentDictionary<byte[], Channel> active_peers = new ConcurrentDictionary<byte[], Channel>();
        private ConcurrentDictionary<IPAddress, Node> trust_nodes = new ConcurrentDictionary<IPAddress, Node>();
        private ConcurrentDictionary<IPAddress, Node> active_nodes = new ConcurrentDictionary<IPAddress, Node>();
        private ConcurrentDictionary<IPAddress, Node> fast_forward_nodes = new ConcurrentDictionary<IPAddress, Node>();
        #endregion


        #region Property
        public static ChannelManager Instance
        {
            get { return instance ?? new ChannelManager(); }
        }

        public ICollection<Channel> ActivePeer
        {
            get { return this.active_peers.Values; }
        }

        public ObjectCache BadPeers
        {
            get { return this.bad_peers; }
        }

        public ObjectCache RecentlyDisconnected
        {
            get { return this.recently_disconnected; }
        }

        public ConcurrentDictionary<IPAddress, Node> TrustNodes
        {
            get { return this.trust_nodes; }
        }

        public ConcurrentDictionary<IPAddress, Node> ActiveNodes
        {
            get { return this.active_nodes; }
        }

        public ConcurrentDictionary<IPAddress, Node> FastForwardNodes
        {
            get { return this.fast_forward_nodes; }
        }
        #endregion


        #region Constructor
        private ChannelManager() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            if (Args.Instance.Node.ListenPort > 0)
            {
                new Thread(new ThreadStart(() =>
                {
                    this.peer_server.Start(Args.Instance.Node.ListenPort);
                })).Start();
            }

            IPAddress address;
            foreach (Node node in Args.Instance.Node.Passive)
            {
                address = new IPEndPoint(new IPAddress(Encoding.UTF8.GetBytes(node.Host)), node.Port).Address;
                this.trust_nodes.TryAdd(address, node);
            }

            foreach (Node node in Args.Instance.Node.Active)
            {
                address = new IPEndPoint(new IPAddress(Encoding.UTF8.GetBytes(node.Host)), node.Port).Address;
                this.trust_nodes.TryAdd(address, node);
                this.active_nodes.TryAdd(address, node);
            }

            foreach (Node node in Args.Instance.Node.FastForward)
            {
                address = new IPEndPoint(new IPAddress(Encoding.UTF8.GetBytes(node.Host)), node.Port).Address;
                this.trust_nodes.TryAdd(address, node);
                this.fast_forward_nodes.TryAdd(address, node);
            }

            Logger.Info(
                string.Format("Node config, trust {0}, active {1}, forward {2}.",
                              this.trust_nodes.Count,
                              this.active_nodes.Count,
                              this.fast_forward_nodes.Count));

            this.sync_pool.init();
            fastForward.init();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool ProcessPeer(Channel peer)
        {
            if (!this.trust_nodes.ContainsKey(peer.SocketAddress))
            {
                if (this.recentlyDisconnected.getIfPresent(peer) != null)
                {
                    logger.info("Peer {} recently disconnected.", peer.getInetAddress());
                    return false;
                }

                if (badPeers.getIfPresent(peer) != null)
                {
                    peer.disconnect(peer.getNodeStatistics().getDisconnectReason());
                    return false;
                }

                if (!peer.isActive() && activePeers.size() >= maxActivePeers)
                {
                    peer.disconnect(TOO_MANY_PEERS);
                    return false;
                }

                if (getConnectionNum(peer.getInetAddress()) >= getMaxActivePeersWithSameIp)
                {
                    peer.disconnect(TOO_MANY_PEERS_WITH_SAME_IP);
                    return false;
                }
            }

            Channel channel = activePeers.get(peer.getNodeIdWrapper());
            if (channel != null)
            {
                if (channel.getStartTime() > peer.getStartTime())
                {
                    logger.info("Disconnect connection established later, {}", channel.getNode());
                    channel.disconnect(DUPLICATE_PEER);
                }
                else
                {
                    peer.disconnect(DUPLICATE_PEER);
                    return false;
                }
            }
            activePeers.put(peer.getNodeIdWrapper(), peer);
            logger.info("Add active peer {}, total active peers: {}", peer, activePeers.size());
            return true;
        }

        public int GetConnectionNum(IPAddress address)
        {
            int count = 0;
            
            foreach (Channel channel in this.active_peers.Values)
            {
                if (channel.Address.Equals(address))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion
    }
}
