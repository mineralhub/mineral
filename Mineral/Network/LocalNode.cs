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
using Mineral.Core.Transactions;

namespace Mineral.Network
{
    public class LocalNode : IDisposable
    {
        private int _listenedFlag;
        private TcpListener _tcpListener;
        private IWebHost _wsHost;
        private Thread _threadSyncBlock;
        private Thread _threadAcceptPeers;
        private Thread _threadConnectPeers;

        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        public bool IsSyncing { get { return NetworkManager.Instance.SyncBlockManager.IsSyncing; } private set { NetworkManager.Instance.SyncBlockManager.IsSyncing = value; } }
        public bool IsServiceEnable { get { return !_cancelTokenSource.IsCancellationRequested; } }

        public void Dispose()
        {
            if (0 < _listenedFlag)
            {
                if (_tcpListener != null)
                    _tcpListener.Stop();
            }
            _threadAcceptPeers.Join();
            _threadConnectPeers.Join();
            _threadSyncBlock.Join();
        }

        public void Listen()
        {
            if (Interlocked.Exchange(ref _listenedFlag, 1) == 0)
            {
                Task.Run(() =>
                {
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
                        if (IsSyncing)
                        {
                            _threadSyncBlock = new Thread(SyncBlocks)
                            {
                                IsBackground = true,
                                Name = "Mineral.LocalNode.SyncBlocks"
                            };
                            _threadSyncBlock.Start();
                        }

                        if (0 < tcpPort)
                        {
                            _tcpListener = new TcpListener(IPAddress.Any, tcpPort);
                            _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                            try
                            {
                                _tcpListener.Start();
                                _threadAcceptPeers = new Thread(AcceptPeersLoop)
                                {
                                    IsBackground = true,
                                    Name = "Mineral.LocalNode.AcceptPeersLoop"
                                };
                                _threadAcceptPeers.Start();
                                _threadConnectPeers = new Thread(ConnectToPeersLoop)
                                {
                                    IsBackground = true,
                                    Name = "Mineral.LocalNode.ConnectToPeersLoop"
                                };
                                _threadConnectPeers.Start();
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
                        Logger.Error(e.Message);
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
                TcpRemoteNode node = new TcpRemoteNode(socket);
                OnConnected(node);
            }
        }

        private void ConnectToPeersLoop()
        {
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                HashSet<RemoteNode> peers = NetworkManager.Instance.ConnectedPeers.Clone();
                if (peers.Count < Config.Instance.ConnectPeerMax)
                {
                    Task[] tasks = { };
                    HashSet<IPEndPoint> waitPeers = NetworkManager.Instance.WaitPeers.Clone();
                    if (0 < waitPeers.Count)
                    {
                        IPEndPoint[] eps = waitPeers.Take(Config.Instance.ConnectPeerMax - peers.Count).ToArray();
                        tasks = eps.Select(p => ConnectToPeerAsync(p)).ToArray();
                    }
                    else if (0 < peers.Count)
                    {
                        foreach (RemoteNode node in peers)
                        {
                            node.RequestAddrs();
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

        private void SyncBlocks()
        {
            SyncBlockManager syncMgr = NetworkManager.Instance.SyncBlockManager;
            while (!_cancelTokenSource.IsCancellationRequested && IsSyncing)
            {
                HashSet<RemoteNode> peers = NetworkManager.Instance.ConnectedPeers.Clone();
                if (peers.Count == 0)
                    continue;

                int syncHeight = BlockChain.Instance.Proof.CalcBlockHeight(DateTime.UtcNow.ToTimestamp());
                int headerHeight = BlockChain.Instance.CurrentHeaderHeight;
                if (headerHeight < syncHeight - 1
                    && IsSyncing)
                {
                    int blockHeight = BlockChain.Instance.CurrentBlockHeight;
                    if (Config.Instance.Block.PayloadCapacity <= headerHeight - blockHeight)
                        continue;

                    NodeInfo info = null;
                    IEnumerable<RemoteNode> orderby = peers
                            .Where(p => 0 < p.Latency)
                            .OrderBy(p => p.Latency);
                    if (orderby.Any())
                    {
                        foreach (RemoteNode node in orderby)
                        {
                            if (headerHeight < node.Height &&
                                syncMgr.SyncRequest(node.Info))
                            {
                                info = node.Info;
                                var start = headerHeight + 1;
                                var end = start + Config.Instance.Block.PayloadCapacity;
                                var payload = GetBlocksFromHeightPayload.Create(start, end);
                                node.EnqueueMessage(Message.CommandName.RequestBlocksFromHeight, payload);
                                break;
                            }
                        }
                    }
                    long t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    while (info == null || syncMgr.ContainsInfo(info))
                    {
                        Thread.Sleep(100);
                        if (5000 < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - t)
                            break;
                    }
                }
                else
                {
                    IsSyncing = false;
                }
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

            NetworkManager.Instance.WaitPeers.Remove(ep);
            TcpRemoteNode node = new TcpRemoteNode(ep);
            if (await node.ConnectAsync())
                OnConnected(node);
        }

        private void OnConnected(RemoteNode node)
        {
            node.DisconnectedCallback += OnDisconnected;
            node.PeersReceivedCallback += OnPeersReceived;
            node.OnConnected();
        }

        private void OnDisconnected(RemoteNode node, DisconnectType type)
        {
            node.DisconnectedCallback -= OnDisconnected;
            node.PeersReceivedCallback -= OnPeersReceived;

            if (node.EndPoint != null)
            {
                /*
                if (type == DisconnectType.InvalidBlock)
                {
                    lock (_badPeers)
                        _badPeers.Add(node.ListenerEndPoint);
                }
                */
            }

            NetworkManager.Instance.ConnectedPeers.Remove(node);
            NetworkManager.Instance.SyncBlockManager.RemoveInfo(node.Info);
        }

        private void OnPeersReceived(RemoteNode node, IPEndPoint[] endPoints)
        {
            HashSet<IPEndPoint> wait = NetworkManager.Instance.WaitPeers.Clone();
            if (Config.Instance.WaitPeerMax < wait.Count)
                return;

            HashSet<RemoteNode> connected = NetworkManager.Instance.ConnectedPeers.Clone();
            HashSet<IPEndPoint> bad = NetworkManager.Instance.BadPeers.Clone();
            wait.UnionWith(endPoints);
            wait.ExceptWith(bad);
            wait.ExceptWith(connected.Select(p => p.EndPoint));
            NetworkManager.Instance.WaitPeers.Add(wait);
        }

        private async Task AcceptWebSocketAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();
            IPEndPoint ep = new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort);
            WebSocketRemoteNode node = new WebSocketRemoteNode(ws, ep);
            OnConnected(node);
        }

        public bool AddTransaction(Transaction tx, bool bBroadcast = true)
        {
            BlockChain.Instance.AddTransactionPool(tx);
            if (bBroadcast)
                BroadCast(Message.CommandName.BroadcastTransactions, TransactionsPayload.Create(tx));
            return true;
        }

        public void RemoveTransactionPool(List<Transaction> txs)
        {
            BlockChain.Instance.RemoveTransactionPool(txs);
        }

        public void BroadCast(Message.CommandName name, ISerializable payload = null)
        {
            var peers = NetworkManager.Instance.ConnectedPeers.Clone();
            foreach (RemoteNode node in peers)
                node.EnqueueMessage(name, payload);
        }
    }
}