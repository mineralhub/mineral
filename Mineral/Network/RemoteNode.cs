using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Mineral.Core;
using Mineral.Network.Payload;
using System.IO;
using Mineral.Utils;

namespace Mineral.Network
{
    public abstract class RemoteNode : IDisposable
    {
        struct PingPong 
        {
            public static readonly int LoopTimeSecond = 10;

            public long LatencyMs { get; private set; }
            public bool Waiting { get; private set; }
            public int LastPongTime { get; private set; }

            public void Ping()
            {
                Waiting = true;
            }

            public void Pong(PongPayload payload)
            {
                LatencyMs = payload.LatencyMs;
                Waiting = false;
                LastPongTime = DateTime.UtcNow.ToTimestamp();
            }

            public bool IsCheckTime => 
                Waiting == false 
                && LastPongTime + LoopTimeSecond <= DateTime.UtcNow.ToTimestamp();
        }

        private static readonly TimeSpan HalfMinute = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan HalfHour = TimeSpan.FromMinutes(30);

        public event Action<RemoteNode, DisconnectType> DisconnectedCallback;
        public event Action<RemoteNode, IPEndPoint[]> PeersReceivedCallback;

        private Queue<Message> _messageQueueHigh = new Queue<Message>();
        private Queue<Message> _messageQueueLow = new Queue<Message>();
        protected int _connected;
        private int _height;
        public int Height { get { return Interlocked.CompareExchange(ref _height, 0, 0); } }

        public bool IsConnected => _connected == 1 ? true : false;
        public VersionPayload Version { get; private set; }
        public IPEndPoint RemoteEndPoint { get; protected set; }
        public IPEndPoint ListenerEndPoint { get; protected set; }

        PingPong _pingpong = new PingPong();
        public long Latency => _pingpong.LatencyMs;

        public RemoteNode(IPEndPoint remoteEndPoint = null)
        {
            RemoteEndPoint = remoteEndPoint;
        }

        internal virtual void OnConnected()
        {
            if (Interlocked.Exchange(ref _connected, 1) == 0)
            {
#if DEBUG
                Logger.Debug("OnConnected. RemoteEndPoint : " + RemoteEndPoint);
#endif
                NetworkProcessAsyncLoop();
            }
            else
            {
                Disconnect(DisconnectType.Exception, "Failed Interlocked.Exchange.");
            }
        }

        public virtual void Disconnect(DisconnectType type, string log)
        {
            if (Interlocked.Exchange(ref _connected, 0) == 1)
            {
#if DEBUG
                Logger.Debug("OnDisconnected. RemoteEndPoint : " + RemoteEndPoint + "\nType : " + type.ToString() + "\nLog : " + log);
#endif
                DisconnectedCallback.Invoke(this, type);
            }
        }

        public virtual void Dispose() 
        {
            Disconnect(DisconnectType.None, "Dispose");
        }

        public void EnqueueMessage(Message.CommandName command, ISerializable payload = null)
        {
            bool single = false;
            switch (command)
            {
                case Message.CommandName.RequestHeaders:
                case Message.CommandName.RequestBlocks:
                    single = true;
                    break;
            }

            Queue<Message> messageQueue;
            switch (command)
            {
                case Message.CommandName.Alert:
                    messageQueue = _messageQueueHigh;
                    break;
                default:
                    messageQueue = _messageQueueLow;
                    break;
            }

            lock (messageQueue)
            {
                if (!single || messageQueue.All(p => p.Command != command))
                    messageQueue.Enqueue(Message.Create(command, payload));
            }
        }

        internal void RequestAddrs()
        {
            EnqueueMessage(Message.CommandName.RequestAddrs);
        }

        private void ReceivedAddrs(AddrPayload payload)
        {
            var peers = payload.AddressList.Select(p => p.EndPoint).Where(
                p => p.Port != Config.Instance.Network.TcpPort || !Config.Instance.LocalAddresses.Contains(p.Address)).ToArray();
            if (0 < peers.Length)
                PeersReceivedCallback.Invoke(this, peers);
        }

        private void ReceivedRequestAddrs()
        {
            HashSet<RemoteNode> connectedPeers = NetworkManager.Instance.ConnectedPeers.Clone();
            IEnumerable<RemoteNode> hostPeers = connectedPeers.Where(p => p.ListenerEndPoint != null && p.Version != null);
            List<AddressInfo> addrs = hostPeers.Select(p => AddressInfo.Create(p.ListenerEndPoint, p.Version.Version, p.Version.Timestamp)).ToList();
            EnqueueMessage(Message.CommandName.ResponseAddrs, AddrPayload.Create(addrs));
        }

        private void ReceivedRequestHeaders(GetBlocksPayload payload)
        {
            List<BlockHeader> headers = new List<BlockHeader>();
            UInt256 hash = payload.HashStart;
            do
            {
                BlockHeader header = BlockChain.Instance.GetNextHeader(hash);
                if (header == null)
                    break;
                headers.Add(header);
                hash = header.Hash;
            }
            while (hash != null && hash != payload.HashStop && headers.Count < HeadersPayload.MaxCount);
            EnqueueMessage(Message.CommandName.ResponseHeaders, HeadersPayload.Create(headers));
        }

        private void ReceivedRequestBlocks(GetBlocksPayload payload)
        {
            List<Block> blocks = new List<Block>();
            UInt256 hash = payload.HashStart;
            do
            {
                Block block = BlockChain.Instance.GetNextBlock(hash);
                if (block == null)
                    break;
                blocks.Add(block);
                hash = block.Hash;
            }
            while (hash != null && hash != payload.HashStop && blocks.Count < BlocksPayload.MaxCount);
            EnqueueMessage(Message.CommandName.ResponseBlocks, BlocksPayload.Create(blocks));
        }

        private void ReceivedRequestBlocksFromHeight(GetBlocksFromHeightPayload payload)
        {
            List<Block> blocks = BlockChain.Instance.GetBlocks(payload.Start, payload.End == 0 ? payload.Start + BlocksPayload.MaxCount : payload.End);
            EnqueueMessage(Message.CommandName.ResponseBlocks, BlocksPayload.Create(blocks));
        }

        private void ReceivedResponseBlocks(BlocksPayload payload)
        {
            foreach (Block block in payload.Blocks)
            {
                BlockChain.ERROR_BLOCK err = BlockChain.Instance.AddBlock(block);
                if (err != BlockChain.ERROR_BLOCK.NO_ERROR &&
                    err != BlockChain.ERROR_BLOCK.ERROR_HEIGHT)
                {
                    Disconnect(DisconnectType.InvalidBlock, "Failed AddResponseBlocks.");
                    break;
                }
            }
            if (!NetworkManager.Instance.SyncBlockManager.SetSyncResponse(Version.NodeID))
                return;
        }

        private void ReceivedBroadcastBlocks(BroadcastBlockPayload payload)
        {
            if (!NetworkManager.Instance.SyncBlockManager.IsSyncing)
            {
                foreach (Block block in payload.Blocks)
                {
                    BlockChain.ERROR_BLOCK err = BlockChain.Instance.AddBlock(block);
                    if (err != BlockChain.ERROR_BLOCK.NO_ERROR &&
                        err != BlockChain.ERROR_BLOCK.ERROR_HEIGHT)
                    {
                        Disconnect(DisconnectType.InvalidBlock, "Failed AddBroadcastBlocks.");
                        break;
                    }
                }
            }
        }

        private void ReceivedBroadcastTransactions(TransactionsPayload payload)
        {
            BlockChain.Instance.AddTransactionPool(payload.Transactions);
        }

        private void OnMessageReceived(Message message)
        {
#if DEBUG
            Logger.Debug(message.Command.ToString());
#endif
            switch (message.Command)
            {
                case Message.CommandName.Version:
                case Message.CommandName.Verack:
                    Disconnect(DisconnectType.InvalidMessageFlow, "OnMessageReceived " + message.Command);
                    break;

                case Message.CommandName.Ping:
                    PingPayload ping = message.Payload.Serializable<PingPayload>();
                    EnqueueMessage(Message.CommandName.Pong, PongPayload.Create(ping.Timestamp, BlockChain.Instance.CurrentBlockHeight));
                    break;

                case Message.CommandName.Pong:
                    {
                        PongPayload pong = message.Payload.Serializable<PongPayload>();
                        Interlocked.Exchange(ref _height, pong.Height);
                        _pingpong.Pong(pong);
                    }
                    break;

                case Message.CommandName.RequestAddrs:
                    ReceivedRequestAddrs();
                    break;
                case Message.CommandName.ResponseAddrs:
                    ReceivedAddrs(message.Payload.Serializable<AddrPayload>());
                    break;
                case Message.CommandName.RequestHeaders:
                    ReceivedRequestHeaders(message.Payload.Serializable<GetBlocksPayload>());
                    break;
                case Message.CommandName.RequestBlocks:
                    ReceivedRequestBlocks(message.Payload.Serializable<GetBlocksPayload>());
                    break;
                case Message.CommandName.RequestBlocksFromHeight:
                    ReceivedRequestBlocksFromHeight(message.Payload.Serializable<GetBlocksFromHeightPayload>());
                    break;
                case Message.CommandName.ResponseHeaders:
                    break;
                case Message.CommandName.ResponseBlocks:
                    ReceivedResponseBlocks(message.Payload.Serializable<BlocksPayload>());
                    break;
                case Message.CommandName.BroadcastBlocks:
                    ReceivedBroadcastBlocks(message.Payload.Serializable<BroadcastBlockPayload>());
                    break;
                case Message.CommandName.BroadcastTransactions:

                case Message.CommandName.Alert:
                    break;
            }
        }

        protected abstract Task<Message> ReceiveMessageAsync(TimeSpan timeout);
        protected abstract Task<bool> SendMessageAsync(Message message);

        private async void NetworkSendProcessAsyncLoop()
        {
#if !NET47
            await Task.Yield();
#endif
            while (IsConnected)
            {
                Message message = null;
                lock (_messageQueueHigh)
                {
                    if (0 < _messageQueueHigh.Count)
                        message = _messageQueueHigh.Dequeue();
                }
                if (message == null)
                {
                    lock (_messageQueueLow)
                    {
                        if (0 < _messageQueueLow.Count)
                            message = _messageQueueLow.Dequeue();
                    }
                }
                if (message == null)
                {
                    for (int i = 0; i < 10 && IsConnected; ++i)
                        Thread.Sleep(100);
                }
                else
                {
                    await SendMessageAsync(message);
                }
            }
        }

        private async void PingPongAsyncLoop()
        {
#if !NET47
            await Task.Yield();
#endif
            while (IsConnected)
            {
                if (_pingpong.IsCheckTime) 
                {
                    _pingpong.Ping();
                    EnqueueMessage(Message.CommandName.Ping, PingPayload.Create());
                }
                Thread.Sleep(100);
            }
        }

        internal async void NetworkProcessAsyncLoop()
        {
#if !NET47
            await Task.Yield();
#endif
            ushort port = 0 < Config.Instance.Network.TcpPort ? Config.Instance.Network.TcpPort : Config.Instance.Network.WsPort;
            if (!await SendMessageAsync(Message.Create(Message.CommandName.Version, VersionPayload.Create(port, NetworkManager.Instance.NodeID))))
                return;

            Message message = await ReceiveMessageAsync(TimeSpan.FromMinutes(5));
            if (message == null)
                return;

            if (message.Command != Message.CommandName.Version)
            {
                Disconnect(DisconnectType.InvalidMessageFlow, "message.Command != Message.CommandName.Version");
                return;
            }
            try
            {
                Version = message.Payload.Serializable<VersionPayload>();
            }
            catch (EndOfStreamException)
            {
                Disconnect(DisconnectType.Exception, "VersionPayload EndOfStreamException");
                return;
            }
            catch (FormatException)
            {
                Disconnect(DisconnectType.Exception, "VersionPayload FormatException");
                return;
            }

            if (ListenerEndPoint != null)
            {
                if (ListenerEndPoint.Port != Version.Port)
                {
                    Disconnect(DisconnectType.InvalidData, "ListenerEndPoint.Port != Version.Port");
                    return;
                }
            }
            else if (0 < Version.Port)
            {
                ListenerEndPoint = new IPEndPoint(RemoteEndPoint.Address, Version.Port);
            }

            if (NetworkManager.Instance.ConnectedPeers.HasPeer(this))
            {
                Disconnect(DisconnectType.MultiConnection, "HasPeer");
                return;
            }

#if DEBUG
            Logger.Debug("Version : " + ListenerEndPoint + ", " + Version.NodeID);
#endif
            if (!await SendMessageAsync(Message.Create(Message.CommandName.Verack, VerackPayload.Create(NetworkManager.Instance.NodeID))))
                return;

            message = await ReceiveMessageAsync(HalfMinute);
            if (message == null)
                return;

            if (message.Command != Message.CommandName.Verack)
            {
                Disconnect(DisconnectType.InvalidMessageFlow, "message.Command != Message.CommandName.Verack");
                return;
            }

            VerackPayload verack = message.Payload.Serializable<VerackPayload>();

            NetworkSendProcessAsyncLoop();
            PingPongAsyncLoop();

            while (IsConnected)
            {
                TimeSpan timeout = HalfHour;
                message = await ReceiveMessageAsync(timeout);
                if (message == null)
                    break;

                try
                {
                    OnMessageReceived(message);
                }
                catch (EndOfStreamException e)
                {

                    Logger.Error(e.Message + "\n" + e.StackTrace);
                    Disconnect(DisconnectType.Exception, message.Command + ". EndOfStreamException");
                    break;
                }
                catch (FormatException)
                {
                    Disconnect(DisconnectType.Exception, message.Command + ". FormatException");
                    break;
                }
            }
        }
    }
}
