using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Mineral.Core;
using Mineral.Network.Payload;
using System.Diagnostics;

namespace Mineral.Network
{
    public class LocalNode : IDisposable
    {
        private int _listenedFlag;
        private int _disposedFlag;
        private TcpListener _tcpListener;
        private IWebHost _wsHost;
        private List<RemoteNode> _connectedPeers = new List<RemoteNode>();
        private List<RemoteNode> _validPeers = new List<RemoteNode>();
        private HashSet<IPEndPoint> _waitPeers = new HashSet<IPEndPoint>();
        private HashSet<IPEndPoint> _badPeers = new HashSet<IPEndPoint>();
        private HashSet<IPEndPoint> _localPoints = new HashSet<IPEndPoint>();

        private Thread _connectThread;
        private Thread _syncThread;

        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        private Guid nodeID = Guid.NewGuid();

        public Guid NodeID { get { return nodeID; } }
        public Dictionary<Guid, IPEndPoint> NodeSet = new Dictionary<Guid, IPEndPoint>();

        private Stopwatch swPing = new Stopwatch();
        public UInt256 lastAddHash = UInt256.Zero;
        public int lastAddHeight = 0;
        public bool _isSyncing = true;
        public List<KeyValuePair<Guid, Block>> broadcastBlocks = new List<KeyValuePair<Guid, Block>>();
        private readonly object _respLock = new Object();

        public long lastBlockTime = 0;
        public long LastTime { get { return swPing.ElapsedMilliseconds; } }

        public bool IsServiceEnable { get { return !_cancelTokenSource.IsCancellationRequested; } }

        public LocalNode()
        {
            _isSyncing = Config.Instance.Block.syncCheck;
            _connectThread = new Thread(ConnectToPeersLoop)
            {
                IsBackground = true,
                Name = "Mineral.LocalNode.ConnectToPeersLoop"
            };
            if (_isSyncing) 
            {
                _syncThread = new Thread(SyncBlocks)
                {
                    IsBackground = true,
                    Name = "Mineral.LocalNode.SyncBlocks"
                };
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposedFlag, 1) == 0)
            {
                if (0 < _listenedFlag)
                {
                    if (_tcpListener != null)
                        _tcpListener.Stop();
                }
            }
        }

        public void Listen()
        {
            if (Interlocked.Exchange(ref _listenedFlag, 1) == 0)
            {
                Task.Run(() =>
                {
                    swPing.Restart();
                    int tcpPort = Config.Instance.Network.TcpPort;
                    int wsPort = Config.Instance.Network.WsPort;
                    try
                    {
                        if (UPNP.Enable)
                        {
                            if (0 < tcpPort || 0 < wsPort)
                            {
                                if (0 < tcpPort)
                                    UPNP.PortMapping(tcpPort, ProtocolType.Tcp, "MINERAL-TCP");
                                if (0 < wsPort)
                                    UPNP.PortMapping(wsPort, ProtocolType.Tcp, "MINERAL-WEBSOCKET");
                            }
                        }
                        _connectThread.Start();
                        if (_syncThread != null)
                            _syncThread.Start();

                        if (0 < tcpPort)
                        {
                            _tcpListener = new TcpListener(IPAddress.Any, tcpPort);
                            _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                            try
                            {
                                _tcpListener.Start();
                                Task.Run(() => AcceptPeersLoop());
                                Task.Run(() => ConnectToPeersLoop());
                            }
                            catch (SocketException) { }
                        }

                        if (0 < wsPort)
                        {
                            _wsHost = new WebHostBuilder().UseKestrel().UseUrls($"http://*:{wsPort}").Configure(app => app.UseWebSockets().Run(AcceptWebSocketAsync)).Build();
                            _wsHost.Start();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message);
                    }
                });
            }
        }

        private void AcceptPeersLoop()
        {
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                Socket socket;
                try
                {
                    socket = _tcpListener.AcceptSocket();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    continue;
                }
                TcpRemoteNode node = new TcpRemoteNode(this, socket);
                OnConnected(node);
            }
        }

        private void ConnectToPeersLoop()
        {
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                int connectedCount = 0;
                lock (_connectedPeers)
                    connectedCount = _connectedPeers.Count;

                if (connectedCount < Config.Instance.ConnectPeerMax)
                {
                    Task[] tasks = { };
                    int waitCount = 0;
                    lock (_waitPeers)
                        waitCount = _waitPeers.Count;

                    if (0 < waitCount)
                    {
                        IPEndPoint[] eps;
                        lock (_waitPeers)
                            eps = _waitPeers.Take(Config.Instance.ConnectPeerMax - connectedCount).ToArray();
                        tasks = eps.Select(p => ConnectToPeerAsync(p)).ToArray();
                    }
                    else if (0 < connectedCount)
                    {
                        lock (_connectedPeers)
                        {
                            foreach (RemoteNode node in _connectedPeers)
                            {
                                node.RequestAddrs();
                            }
                        }
                    }
                    else if (Config.Instance.Network.SeedList != null)
                    {
                        var split = Config.Instance.Network.SeedList.OfType<string>().Select(p => p.Split(':'));
                        tasks = split.Select(p => ConnectToPeerAsync(p[0], int.Parse(p[1]))).ToArray();
                    }

                    try
                    {
                        Task.WaitAll(tasks, _cancelTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                for (int i = 0; i < 50 && !_cancelTokenSource.IsCancellationRequested; ++i)
                    Thread.Sleep(100);
            }
        }

        public bool AddBroadcastTransactions(List<Transaction> transactions, RemoteNode node)
        {
            lock (_respLock)
            {
                Blockchain.Instance.AddTransactionPool(transactions);
                return true;
            }
        }

        public bool AddBroadcastBlocks(List<Block> blocks, RemoteNode node)
        {
            if (_isSyncing)
                return true;

            foreach (Block block in blocks)
            {
                Blockchain.BLOCK_ERROR err = Blockchain.Instance.AddBlock(block);
                if (err != Blockchain.BLOCK_ERROR.E_NO_ERROR)
                    Logger.Log("[error] AddBroadcastBlocks failed. error : " + err);
            }
            return true;
        }

        public bool AddResponseBlocks(List<Block> blocks, RemoteNode node)
        {
            foreach (Block block in blocks)
            {
                Blockchain.BLOCK_ERROR eRET = Blockchain.Instance.AddBlock(block);
                if (eRET != Blockchain.BLOCK_ERROR.E_NO_ERROR)
                {
                    Logger.Log("Blockchain.Instance.AddBlock(block) failed. : " + eRET);
                    continue;
                }
                lastAddHash = block.Hash;
            }
            return true;
        }

        private void SyncBlocks()
        {
            lastAddHash = Blockchain.Instance.CurrentBlockHash;
            while (!_cancelTokenSource.IsCancellationRequested && _isSyncing)
            {
                int connectedCount = 0;
                lock (_connectedPeers)
                    connectedCount = _connectedPeers.Count;

                int syncHeight = Blockchain.Instance.Proof.CalcBlockHeight(DateTime.UtcNow.ToTimestamp());
                int localHeight = Blockchain.Instance.CurrentBlockHeight;
                if (connectedCount == 0)
                    continue;

                if (localHeight < syncHeight - 1
                    && _isSyncing)
                {
                    lock (_connectedPeers)
                    {
                        IEnumerable<RemoteNode> nodes = _connectedPeers
                            .Where(p => 0 < p.Latency)
                            .OrderBy(p => p.Latency);
                        if (nodes.Any())
                            nodes.First().EnqueueMessage(Message.CommandName.RequestBlocksFromHeight, GetBlocksFromHeightPayload.Create(Blockchain.Instance.CurrentHeaderHeight + 1));
                    }
                }
                else
                {
                    _isSyncing = false;
                }
                Thread.Sleep(5000);
            }
        }

        public async Task ConnectToPeerAsync(string host, int port)
        {
            IPAddress addr = null;
            if (IPAddress.TryParse(host, out addr))
            {
                addr = addr.MapToIPv6();
            }
            else
            {
                IPHostEntry entry;
                try
                {
                    entry = await Dns.GetHostEntryAsync(host);
                }
                catch (SocketException)
                {
                    return;
                }
                addr = entry.AddressList.FirstOrDefault(p => p.AddressFamily == AddressFamily.InterNetwork || p.IsIPv6Teredo)?.MapToIPv6();
                if (addr == null)
                    return;
            }
            await ConnectToPeerAsync(new IPEndPoint(addr, port));
        }

        public async Task ConnectToPeerAsync(IPEndPoint ep)
        {
            if (ep.Port == Config.Instance.Network.TcpPort && Config.Instance.LocalAddresses.Contains(ep.Address))
                return;

            lock (_waitPeers)
                _waitPeers.Remove(ep);

            TcpRemoteNode node = new TcpRemoteNode(this, ep);
            if (await node.ConnectAsync())
                OnConnected(node);
        }

        private void OnConnected(RemoteNode node)
        {
            lock (_connectedPeers)
            {
                if (node.ListenerEndPoint != null && _connectedPeers.Any(p => node.ListenerEndPoint.Equals(p.ListenerEndPoint)))
                {
                    node.Disconnect(false);
                    return;
                }

                _connectedPeers.Add(node);
            }
            node.DisconnectedCallback += OnDisconnected;
            node.PeersReceivedCallback += OnPeersReceived;
            node.OnConnected();
        }

        private void OnDisconnected(RemoteNode node, bool error)
        {
            node.DisconnectedCallback -= OnDisconnected;
            node.PeersReceivedCallback -= OnPeersReceived;
            if (error && node.ListenerEndPoint != null)
            {
                lock (_badPeers)
                    _badPeers.Add(node.ListenerEndPoint);
            }

            lock (_connectedPeers)
                _connectedPeers.Remove(node);
        }

        private void OnPeersReceived(RemoteNode node, IPEndPoint[] endPoints)
        {
            lock (_waitPeers)
            {
                if (_waitPeers.Count < Config.Instance.WaitPeerMax)
                {
                    lock (_badPeers)
                    {
                        lock (_connectedPeers)
                        {
                            _waitPeers.UnionWith(endPoints);
                            _waitPeers.ExceptWith(_localPoints);
                            _waitPeers.ExceptWith(_badPeers);
                            _waitPeers.ExceptWith(_connectedPeers.Select(p => p.ListenerEndPoint));
                        }
                    }
                }
            }
        }

        private async Task AcceptWebSocketAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();
            IPEndPoint ep = new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort);
            WebSocketRemoteNode node = new WebSocketRemoteNode(ws, this, ep);
            OnConnected(node);
        }

        public bool HasPeer(RemoteNode node)
        {
            lock (_connectedPeers)
            {
                return _connectedPeers.Where(p => p != node && p.ListenerEndPoint != null).Any(
                    p => p.ListenerEndPoint.Address.Equals(node.ListenerEndPoint) && p.Version?.Nonce == node.Version.Nonce);
            }
        }

        public bool HasNode(RemoteNode node, bool bAdd = false)
        {
            if (NodeID.Equals(node.Version.NodeID))
                return true;

            lock (NodeSet)
            {
                if (NodeSet.ContainsKey(node.Version.NodeID))
                    return true;

                if (bAdd)
                    NodeSet.Add(node.Version.NodeID, node.ListenerEndPoint);
                return false;
            }
        }

        public List<RemoteNode> CloneConnectedPeers()
        {
            lock (_connectedPeers)
            {
                return new List<RemoteNode>(_connectedPeers);
            }
        }

        public bool AddTransaction(Transaction tx, bool bBroadcast = true)
        {
            Blockchain.Instance.AddTransactionPool(tx);
            if (bBroadcast)
                BroadCast(Message.CommandName.BroadcastTransactions, TransactionsPayload.Create(tx));
            return true;
        }

        public void RemoveTransactionPool(List<Transaction> txs)
        {
            Blockchain.Instance.RemoveTransactionPool(txs);
        }

        public void BroadCast(Message.CommandName name, ISerializable payload = null)
        {
            lock (_connectedPeers)
            {
                foreach (RemoteNode node in _connectedPeers)
                    node.EnqueueMessage(name, payload);
            }
        }
    }
}