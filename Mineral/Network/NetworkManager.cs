using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

    public class NodeInfo : IEquatable<NodeInfo>, ISerializable
    {
        public IPEndPoint EndPoint;
        public VersionPayload Version;

        public int Size => 16 + sizeof(ushort) + Version.Size;

        public bool Equals(NodeInfo other)
        {
            return EndPoint == other.EndPoint && EndPoint.Port == other.EndPoint.Port
                && Version.NodeID == other.Version.NodeID;
        }

        public override int GetHashCode()
        {
            return EndPoint.GetHashCode() + Version.GetHashCode();
        }

        public void Deserialize(BinaryReader reader)
        {
            IPAddress addr = new IPAddress(reader.ReadBytes(16));
            ushort port = reader.ReadBytes(2).ToArray().ToUInt16(0);
            EndPoint = new IPEndPoint(addr, port);
            Version = reader.ReadSerializable<VersionPayload>();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(EndPoint.Address.GetAddressBytes());
            writer.Write(BitConverter.GetBytes((ushort)EndPoint.Port).ToArray());
            writer.WriteSerializable(Version);
        }
    }


    public class NetworkManager
    {
        static private NetworkManager _instance = new NetworkManager();
        static public NetworkManager Instance => _instance;

        public ConcurrentDictionary<NodeInfo, RemoteNode> ConnectedPeers { get; } = new ConcurrentDictionary<NodeInfo, RemoteNode>();
        public ConcurrentDictionary<IPEndPoint, int> WaitPeers { get; } = new ConcurrentDictionary<IPEndPoint, int>();
        public ConcurrentDictionary<IPEndPoint, int> BadPeers { get; } = new ConcurrentDictionary<IPEndPoint, int>();
        public SyncBlockManager SyncBlockManager { get; } = new SyncBlockManager();
        public Guid NodeID { get; } = Guid.NewGuid();
        public List<NodeInfo> LocalInfos { get; } = new List<NodeInfo>();

        public NetworkManager()
        {
            NodeInfo info = new NodeInfo();
            info.Version = new VersionPayload();
            ushort tcpPort = Config.Instance.Network.TcpPort;
            foreach (var addr in Config.Instance.LocalAddresses)
            {
                info.EndPoint = new IPEndPoint(addr, tcpPort);
                LocalInfos.Add(info);
            }
        }
    }
}
