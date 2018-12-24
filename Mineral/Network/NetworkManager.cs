using System;
using System.Collections.Generic;
using System.Linq;

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

    public class NetworkManager
    {
        static private NetworkManager _instance = new NetworkManager();
        static public NetworkManager Instance => _instance;

        private List<RemoteNode> _connectedPeers = new List<RemoteNode>();
        public SyncBlockManager SyncBlockManager { get; } = new SyncBlockManager();

        public List<RemoteNode> CloneConnectedPeers()
        {
            List<RemoteNode> nodes;
            lock (_connectedPeers)
                nodes = new List<RemoteNode>(_connectedPeers);
            return nodes;
        }

        public void AddConnectedPeer(RemoteNode node)
        {
            lock (_connectedPeers)
                _connectedPeers.Add(node);
        }

        public void RemoveConnectedPeer(RemoteNode node)
        {
            lock (_connectedPeers)
                _connectedPeers.Remove(node);
        }

        public bool HasPeer(RemoteNode node)
        {
            bool has = false;
            lock (_connectedPeers)
            {
                has = _connectedPeers.Where(p => p != node && p.ListenerEndPoint != null).Any(
                    p => p.ListenerEndPoint == node.ListenerEndPoint && p.Version?.NodeID == node.Version?.NodeID);
            }
            return has;
        }
    }
}
