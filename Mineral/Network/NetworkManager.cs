using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Mineral.Network
{
    public enum SyncBlockState : short
    {
        None,
        Request,
        Response
    }

    public class SyncBlockManager
    {
        private object _syncBlockLock = new object();
        private SyncBlockState _syncBlockState;
        private Guid _syncRequestNodeId;

        public void SetSyncRequest(Guid guid)
        {
            lock (_syncBlockLock)
            {
                _syncBlockState = SyncBlockState.Request;
                _syncRequestNodeId = guid;
            }
        }

        public SyncBlockState GetSyncBlockState()
        {
            SyncBlockState retval;
            lock (_syncBlockLock)
                retval = _syncBlockState;
            return retval;
        }

        public bool SetSyncResponse(Guid guid)
        {
            lock (_syncBlockLock)
            {
                if (_syncRequestNodeId == guid)
                {
                    _syncRequestNodeId = Guid.Empty;
                    _syncBlockState = SyncBlockState.Response;
                    return true;
                }
            }
            return false;
        }

        public void SetSyncCancel()
        {
            lock (_syncBlockLock)
            {
                _syncRequestNodeId = Guid.Empty;
                _syncBlockState = SyncBlockState.Response;
            }
        }
    }

    public class SafePeerList<T>
    {
        protected HashSet<T> _list = new HashSet<T>();

        public void Add(T v) { lock (_list) _list.Add(v); }
        public void Add(HashSet<T> v) { lock (_list) _list.UnionWith(v); }
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
                has = _list.Where(p => p != node && p.ListenerEndPoint != null).Any(
                    p => p.ListenerEndPoint == node.ListenerEndPoint && p.Version?.NodeID == node.Version?.NodeID);
            }
            return has;
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
    }
}
