using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Sky.Core;
using Sky.Network.Payload;
using System.IO;

namespace Sky.Network
{
    public abstract class RemoteNode : IDisposable
    {
        protected LocalNode _localNode;

        private static readonly TimeSpan HalfMinute = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan HalfHour = TimeSpan.FromMinutes(30);

        public event Action<RemoteNode, bool> DisconnectedCallback;
        public event Action<RemoteNode, IPEndPoint[]> PeersReceivedCallback;

        private Queue<Message> _messageQueueHigh = new Queue<Message>();
        private Queue<Message> _messageQueueLow = new Queue<Message>();

        public bool IsConnected => _connected == 1 ? true : false;
        protected int _connected;
        public VersionPayload Version { get; private set; }
        public IPEndPoint RemoteEndPoint { get; protected set; }
        public IPEndPoint ListenerEndPoint { get; protected set; }

        public RemoteNode(LocalNode node, IPEndPoint remoteEndPoint = null)
        {
            _localNode = node;
            RemoteEndPoint = remoteEndPoint;
        }

        internal virtual void OnConnected()
        {
            if (Interlocked.Exchange(ref _connected, 1) == 0)
            {
#if DEBUG
                Logger.Log("OnConnected. RemoteEndPoint : " + RemoteEndPoint);
#endif
                NetworkProcessAsyncLoop();
            }
            else
            {
                Disconnect(false);
            }
        }

        public virtual void Disconnect(bool error)
        {
            if (Interlocked.Exchange(ref _connected, 0) == 1)
            {
#if DEBUG
                Logger.Log("OnDisconnected. RemoteEndPoint : " + RemoteEndPoint);
#endif

                DisconnectedCallback?.Invoke(this, error);
            }
        }

        public virtual void Dispose()
        {
            Disconnect(false);
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
            IPEndPoint[] peers = payload.AddressList.Select(p => p.EndPoint).Where(
                p => p.Port != Config.Network.TcpPort || !Config.LocalAddresses.Contains(p.Address)).ToArray();
            if (0 < peers.Length)
                PeersReceivedCallback?.Invoke(this, peers);
        }

        private void ReceivedRequestAddrs()
        {
            if (!_localNode.IsServiceEnable)
                return;

            List<RemoteNode> connectedPeers = _localNode.CloneConnectedPeers();
            IEnumerable<RemoteNode> hostPeers = connectedPeers.Where(p => p.ListenerEndPoint != null && p.Version != null);
            List<AddressInfo> addrs = hostPeers.Select(p => AddressInfo.Create(p.ListenerEndPoint, p.Version.Version, p.Version.Timestamp)).ToList();
            EnqueueMessage(Message.CommandName.ResponseAddrs, AddrPayload.Create(addrs));
        }

        private void ReceivedRequestHeaders(GetBlocksPayload payload)
        {
            if (!_localNode.IsServiceEnable)
                return;

            List<BlockHeader> headers = new List<BlockHeader>();
            UInt256 hash = payload.HashStart;
            do
            {
                BlockHeader header = Blockchain.Instance.GetNextHeader(hash);
                if (header == null)
                    break;
                headers.Add(header);
            }
            while (hash != null && hash != payload.HashStop && headers.Count < HeadersPayload.MaxCount);
            EnqueueMessage(Message.CommandName.ResponseHeaders, HeadersPayload.Create(headers));
        }

		private void ReceivedRequestBlocks(GetBlocksPayload payload)
		{
			if (!_localNode.IsServiceEnable)
				return;

			List<Block> blocks = new List<Block>();
			UInt256 hash = payload.HashStart;
			do
			{
				Block block  = Blockchain.Instance.GetBlock(hash);
				if (block == null)
					break;
				blocks.Add(block);
			}
			while (hash != null && hash != payload.HashStop && blocks.Count < BlocksPayload.MaxCount);
			EnqueueMessage(Message.CommandName.ResponseBlocks, BlocksPayload.Create(blocks));
		}

		private void ReceivedResponseBlocks(BlocksPayload payload)
		{
			if (!_localNode.IsServiceEnable)
				return;

			foreach(Block block in payload.Blocks)
			{
				Blockchain.Instance.AddBlock(block);
			}
		}

		private void OnMessageReceived(Message message)
        {
            switch (message.Command)
            {
                case Message.CommandName.Version:
                case Message.CommandName.Verack:
                    Disconnect(true);
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
                case Message.CommandName.ResponseHeaders:
					break;
				case Message.CommandName.ResponseBlocks:
					ReceivedResponseBlocks(message.Payload.Serializable<BlocksPayload>());
					break;
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

        internal async void NetworkProcessAsyncLoop()
        {
#if !NET47
            await Task.Yield();
#endif
            ushort port = 0 < Config.Network.TcpPort ? Config.Network.TcpPort : Config.Network.WsPort;
            if (!await SendMessageAsync(Message.Create(Message.CommandName.Version, VersionPayload.Create(port))))
                return;

            Message message = await ReceiveMessageAsync(HalfMinute);
            if (message == null)
                return;

            if (message.Command != Message.CommandName.Version)
            {
                Disconnect(true);
                return;
            }
            try
            {
                Version = message.Payload.Serializable<VersionPayload>();
            }
            catch (EndOfStreamException)
            {
                Disconnect(false);
                return;
            }
            catch (FormatException)
            {
                Disconnect(true);
                return;
            }
            if (ListenerEndPoint != null)
            {
                if (ListenerEndPoint.Port != Version.Port)
                {
                    Disconnect(true);
                    return;
                }
            }
            else if (0 < Version.Port)
            {
                ListenerEndPoint = new IPEndPoint(RemoteEndPoint.Address, Version.Port);
            }

            if (_localNode.HasPeer(this))
            {
                Disconnect(false);
                return;
            }
#if DEBUG
            Logger.Log("Version : " + ListenerEndPoint);
#endif
            if (!await SendMessageAsync(Message.Create(Message.CommandName.Verack)))
                return;

            message = await ReceiveMessageAsync(HalfMinute);
            if (message == null)
                return;

            if (message.Command != Message.CommandName.Verack)
            {
                Disconnect(true);
                return;
            }

//            if (Blockchain.Instance.CurrentHeaderHeight < Version.Height)
//                EnqueueMessage(Message.CommandName.RequestHeaders, GetBlocksPayload.Create(Blockchain.Instance.CurrentHeaderHash));

            NetworkSendProcessAsyncLoop();
            while (IsConnected)
            {
                if (Blockchain.Instance.CurrentBlockHeight < Version.Height)
                    EnqueueMessage(Message.CommandName.RequestBlocks, GetBlocksPayload.Create(Blockchain.Instance.CurrentHeaderHash));

                TimeSpan timeout = HalfHour;
                message = await ReceiveMessageAsync(timeout);
                if (message == null)
                    break;
                    
                try
                {
                    OnMessageReceived(message);
                }
                catch (EndOfStreamException)
                {
                    Disconnect(false);
                    break;
                }
                catch (FormatException)
                {
                    Disconnect(true);
                    break;
                }
            }
        }
    }
}
