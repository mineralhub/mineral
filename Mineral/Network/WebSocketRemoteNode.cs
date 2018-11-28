using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral.Network
{
    class WebSocketRemoteNode : RemoteNode
    {
        private WebSocket _socket;

        public WebSocketRemoteNode(WebSocket ws, LocalNode node, IPEndPoint remoteEndPoint) : base(node, remoteEndPoint)
        {
            _socket = ws;
        }

        public override void Disconnect(bool error, bool removeNode = false)
        {
            if (_socket != null)
                _socket.Dispose();

            base.Disconnect(error, removeNode);
        }

        internal override void OnConnected()
        {
        }

        protected override async Task<Message> ReceiveMessageAsync(TimeSpan timeout)
        {
            using (CancellationTokenSource source = new CancellationTokenSource(timeout))
            {
                try
                {
                    return await Message.DeserializeFromAsync(_socket, source.Token);
                }
                catch (ArgumentException) { }
                catch (ObjectDisposedException) { }
                catch(OperationCanceledException)
                {
                    Disconnect(false, true);
                }
                catch (Exception e) when (e is FormatException || e is IOException || e is WebSocketException)
                {
                    Disconnect(false);
                }
            }
            return null;
        }

        protected override async Task<bool> SendMessageAsync(Message message)
        {
            if (!IsConnected)
                return false;

            ArraySegment<byte> segment = new ArraySegment<byte>(message.ToArray());
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await _socket.SendAsync(segment, WebSocketMessageType.Binary, true, source.Token);
                return true;
            }
            catch (ObjectDisposedException) { }
            catch(OperationCanceledException) {
                Disconnect(false, true);
            }
            catch (Exception ex) when (ex is WebSocketException)
            {
                Disconnect(false);
            }
            finally
            {
                source.Dispose();
            }
            return false;
        }
    }
}
