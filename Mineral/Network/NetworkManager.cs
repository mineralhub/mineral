using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Mineral.Network.Payload;

namespace Mineral.Network
{
    public class SyncBlockManager
    {
        private ConcurrentDictionary<NodeInfo, bool> _syncStates = new ConcurrentDictionary<NodeInfo, bool>();
        private int _isSyncing;
        public bool IsSyncing
        {
            get
            {
                return (Interlocked.CompareExchange(ref _isSyncing, 1, 1) == 1);
            }
            set
            {
                if (value)
                    Interlocked.CompareExchange(ref _isSyncing, 1, 0);
                else
                    Interlocked.CompareExchange(ref _isSyncing, 0, 1);
            }
        }

        public SyncBlockManager()
        {
            IsSyncing = Config.Instance.Block.SyncCheck;
        }

        public bool SyncRequest(NodeInfo info)
        {
             return _syncStates.TryAdd(info, true);
        }

        public bool ContainsInfo(NodeInfo info)
        {
            return _syncStates.ContainsKey(info);
        }

        public void RemoveInfo(NodeInfo info)
        {
            _syncStates.TryRemove(info, out _);
        }
    }

    public class SafePeerList<T>
    {
        protected HashSet<T> _list = new HashSet<T>();

        public virtual bool Add(T v) 
        {
            bool retval = false;
            lock (_list) 
                retval = _list.Add(v);
            return retval;
        }
        public virtual void Add(HashSet<T> v) { lock (_list) _list.UnionWith(v); }
        public void Remove(T v) { lock (_list) _list.Remove(v); }
        public HashSet<T> Clone() 
        {
            HashSet<T> retval;
            lock (_list)
                retval = new HashSet<T>(_list);
            return retval;
        }
    }

    public class ConnectedPeerList : SafePeerList<RemoteNode>
    {
        public bool HasPeer(RemoteNode node)
        {
            bool has = false;
            lock (_list)
            {
                has = _list.Any(p => p.Equals(node));
            }
            return has;
        }
    }

    public class NodeInfo : IEquatable<NodeInfo>
    {
        public IPEndPoint EndPoint;
        public VersionPayload Version;

        public bool Equals(NodeInfo other)
        {
            return EndPoint == other.EndPoint && EndPoint.Port == other.EndPoint.Port
                && Version.NodeID == other.Version.NodeID;
        }

        public override int GetHashCode()
        {
            return EndPoint.GetHashCode() + Version.GetHashCode();
        }
    }


    public class NetworkManager
    {
        static private NetworkManager _instance = new NetworkManager();
        static public NetworkManager Instance => _instance;

        public ConnectedPeerList ConnectedPeers { get; } = new ConnectedPeerList();
        public SafePeerList<IPEndPoint> WaitPeers { get; } = new SafePeerList<IPEndPoint>();
        public SafePeerList<IPEndPoint> BadPeers { get; } = new SafePeerList<IPEndPoint>();
        public SyncBlockManager SyncBlockManager { get; } = new SyncBlockManager();
        public Guid NodeID { get; } = Guid.NewGuid();
    }
}
