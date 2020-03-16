using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Server;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Overlay.Client
{
    public class PeerClient
    {
        #region Field
        private IChannel channel = null;
        private IEventLoopGroup worker_group = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public PeerClient()
        {
            this.worker_group = new MultithreadEventLoopGroup(1);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private async Task<IChannel> ConnectAsync(string host, int port, string remote_id, bool discovery_mode)
        {
            Logger.Info(
                string.Format("connect peer {0} {1} {2}", host, port, remote_id));

            try
            {
                NettyChannelInitializer initializer = new NettyChannelInitializer(remote_id, discovery_mode);

                Bootstrap bootstrap = new Bootstrap();
                bootstrap.Group(this.worker_group);
                bootstrap.Channel<TcpSocketChannel>();
                bootstrap.Option(ChannelOption.SoKeepalive, true);
                bootstrap.Option(ChannelOption.MessageSizeEstimator, DefaultMessageSizeEstimator.Default);
                bootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(Args.Instance.Node.ConnectionTimeout));
                bootstrap.Handler(initializer);

                return await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));
            }
            catch (System.Exception e)
            {
                Logger.Warning(e.Message, e);
            }

            return null;
        }
        #endregion


        #region External Method
        public void Connect(string host, int port, string remote_id)
        {
            try
            {
                this.channel = ConnectAsync(host, port, remote_id, false).Result;
            }
            catch (Exception e)
            {
                Logger.Info("PeerClient: Can't connect to " + host + ":" + port + " (" + e.Message + ")");
            }
        }

        public IChannel ConnectAsync(NodeHandler handler, bool discovery_mode)
        {
            Node node = handler.Node;
            return ConnectAsync(node.Host, node.Port, node.Id.ToHexString(), discovery_mode).Result;
        }

        public void Close()
        {
            if (this.channel != null)
            {
                Logger.Info("Closing peer client...");
                this.channel.CloseAsync();
            }
        }
        #endregion
    }
}