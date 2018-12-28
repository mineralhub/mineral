using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral.Network
{
    public class TcpRemoteNode : RemoteNode
    {
        private Socket _socket;
        private NetworkStream _stream;

        // connect peer
        public TcpRemoteNode(IPEndPoint listenerEndPoint)
        {
            AddressFamily addrf = listenerEndPoint.AddressFamily;
            if (listenerEndPoint.Address.IsIPv4MappedToIPv6)
                addrf = AddressFamily.InterNetwork;
            _socket = new Socket(addrf, SocketType.Stream, ProtocolType.Tcp);
            ListenerEndPoint = listenerEndPoint;
        }

        // accept peer
        public TcpRemoteNode(Socket socket)
            : base(new IPEndPoint(((IPEndPoint)socket.RemoteEndPoint).Address.MapToIPv6(), ((IPEndPoint)socket.RemoteEndPoint).Port))
        {
            _socket = socket;
        }

        public override void Disconnect(DisconnectType type, string log)
        {
            if (_socket != null)
                _socket.Dispose();
            if (_stream != null)
                _stream.Dispose();

            base.Disconnect(type, log);
        }

        internal override void OnConnected()
        {
            if (RemoteEndPoint == null)
            {
                IPEndPoint ep = (IPEndPoint)_socket.RemoteEndPoint;
                RemoteEndPoint = new IPEndPoint(ep.Address.MapToIPv6(), ep.Port);
            }
            _stream = new NetworkStream(_socket);
            base.OnConnected();
        }

        public async Task<bool> ConnectAsync()
        {
            IPAddress addr = ListenerEndPoint.Address;
            if (addr.IsIPv4MappedToIPv6)
                addr = addr.MapToIPv4();

            try
            {
                await _socket.ConnectAsync(addr, ListenerEndPoint.Port);
            }
            catch (SocketException)
            {
                Disconnect(DisconnectType.Exception, "SocketException.");
                return false;
            }
            return true;
        }

        protected override async Task<Message> ReceiveMessageAsync(TimeSpan timeout)
        {
            CancellationTokenSource source = new CancellationTokenSource(timeout);
            source.Token.Register(() => Disconnect(DisconnectType.Exception, "Failed Token Register."));
            try
            {
                return await Message.DeserializeFromAsync(_stream, source.Token);
            }
            catch (Exception e)
            {
                Disconnect(DisconnectType.Exception, e.GetType().ToString());
            }
            finally
            {
                source.Dispose();
            }
            return null;
        }

        protected override async Task<bool> SendMessageAsync(Message message)
        {
            if (!IsConnected)
                return false;

            byte[] buf = message.ToArray();
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            source.Token.Register(() => Disconnect(DisconnectType.Exception, "Failed Token Register."));
            try
            {
                await _stream.WriteAsync(buf, 0, buf.Length, source.Token);
                return true;
            }
            catch (Exception e)
            {
                Disconnect(DisconnectType.Exception, e.GetType().ToString());
            }
            finally
            {
                source.Dispose();
            }
            return false;
        }
    }
}
