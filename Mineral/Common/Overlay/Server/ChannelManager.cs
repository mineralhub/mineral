using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mineral.Common.Overlay.Client;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;
using Protocol;

namespace Mineral.Common.Overlay.Server
{
    using Node = Mineral.Common.Overlay.Discover.Node.Node;

    public class ChannelManager
    {
        #region Field
        private Cache<ReasonCode> bad_peers = new Cache<ReasonCode>("badpeers").ExpireTime(TimeSpan.FromHours(1)).MaxCapacity(10000);
        private Cache<ReasonCode> recently_disconnected = new Cache<ReasonCode>("recently_disconnected").ExpireTime(TimeSpan.FromSeconds(30)).MaxCapacity(1000);

        private ConcurrentDictionary<byte[], Channel> active_peers = new ConcurrentDictionary<byte[], Channel>(new ByteArrayEqualComparer());
        private ConcurrentDictionary<IPAddress, Node> trust_nodes = new ConcurrentDictionary<IPAddress, Node>();
        private ConcurrentDictionary<IPAddress, Node> active_nodes = new ConcurrentDictionary<IPAddress, Node>();
        private ConcurrentDictionary<IPAddress, Node> fast_forward_nodes = new ConcurrentDictionary<IPAddress, Node>();
        #endregion


        #region Property
        public ICollection<Channel> ActivePeer
        {
            get { return this.active_peers.Values; }
        }

        public Cache<ReasonCode> BadPeers
        {
            get { return this.bad_peers; }
        }

        public Cache<ReasonCode> RecentlyDisconnected
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
                Task.Run(() =>
                {
                    Manager.Instance.PeerServer.Start(Args.Instance.Node.ListenPort);
                });
            }

            IPAddress address;
            foreach (Node node in Args.Instance.Node.Passive)
            {
                address = new IPEndPoint(IPAddress.Parse(node.Host), node.Port).Address;
                this.trust_nodes.TryAdd(address, node);
            }

            foreach (Node node in Args.Instance.Node.Active)
            {
                address = new IPEndPoint(IPAddress.Parse(node.Host), node.Port).Address;
                this.trust_nodes.TryAdd(address, node);
                this.active_nodes.TryAdd(address, node);
            }

            foreach (Node node in Args.Instance.Node.FastForward)
            {
                address = new IPEndPoint(IPAddress.Parse(node.Host), node.Port).Address;
                this.trust_nodes.TryAdd(address, node);
                this.fast_forward_nodes.TryAdd(address, node);
            }

            Logger.Info(
                string.Format("Node config, trust {0}, active {1}, forward {2}.",
                              this.trust_nodes.Count,
                              this.active_nodes.Count,
                              this.fast_forward_nodes.Count));

            Manager.Instance.SyncPool.Init();
            Manager.Instance.FastForward.Init();
        }

        public void NotifyDisconnect(Channel channel)
        {
            Manager.Instance.SyncPool.OnDisconnect(channel);
            this.active_peers.TryRemove(channel.Node.Id, out _);

            if (channel != null)
            {
                channel.NodeStatistics?.NodifyDisconnect();

                if (channel.Address != null
                    && GetRecentlyDisconnected(channel.Address) == null)
                {
                    AddRecentlyDisconnected(channel.Address, ReasonCode.Unknown);
                }

                channel = null;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool ProcessPeer(Channel peer)
        {
            if (!this.trust_nodes.ContainsKey(peer.SocketAddress))
            {
                if (GetRecentlyDisconnected(peer.Address) != null)
                {
                    Logger.Info(
                        string.Format("Peer {0} recently disconnected.", peer.Address));

                    return false;
                }

                if (GetBadPeer(peer.Address) != null)
                {
                    peer.Disconnect((ReasonCode)peer.NodeStatistics.GetDisconnectReason(), "");
                    return false;
                }

                if (!peer.IsActive && this.active_peers.Count >= Args.Instance.Node.MaxActiveNodes)
                {
                    peer.Disconnect(ReasonCode.TooManyPeers, "");
                    return false;
                }

                if (GetConnectionNum(peer.Address) >= Args.Instance.Node.MaxActiveNodeSameIP)
                {
                    peer.Disconnect(ReasonCode.TooManyPeersWithSameIp, "");
                    return false;
                }
            }

            this.active_peers.TryGetValue(peer.Node.Id, out Channel channel);
            if (channel != null)
            {
                if (channel.StartTime > peer.StartTime)
                {
                    Logger.Info("Disconnect connection established later, " + channel.Node.ToString());
                    channel.Disconnect(ReasonCode.DuplicatePeer, "");
                }
                else
                {
                    peer.Disconnect(ReasonCode.DuplicatePeer, "");
                    return false;
                }
            }

            this.active_peers.TryAdd(peer.Node.Id, peer);
            Logger.Info(
                string.Format("Add active peer {0}, total active peers: {1}", peer, this.active_peers.Count));

            return true;
        }

        public void ProcessDisconnect(Channel channel, ReasonCode reason)
        {
            if (channel.Address == null)
            {
                return;
            }

            switch (reason)
            {
                case ReasonCode.BadProtocol:
                case ReasonCode.BadBlock:
                case ReasonCode.BadTx:
                    AddBadPeer(channel.Address, reason);
                    break;
                default:
                    AddRecentlyDisconnected(channel.Address, reason);
                    break;
            }
        }

        public void AddBadPeer(IPAddress key, ReasonCode value)
        {
            this.bad_peers.Add(key.ToString(), value);
        }

        public void AddRecentlyDisconnected(IPAddress key, ReasonCode value)
        {
            this.recently_disconnected.Add(key.ToString(), value);
        }

        public object GetBadPeer(IPAddress key)
        {
            return this.bad_peers.Get(key.ToString());
        }

        public object GetRecentlyDisconnected(IPAddress key)
        {
            return this.recently_disconnected.Get(key.ToString());
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

        public void Close()
        {
            Manager.Instance.PeerServer.Close();
            Manager.Instance.PeerClient.Close();
            Manager.Instance.SyncPool.Close();
        }
        #endregion
    }
}
