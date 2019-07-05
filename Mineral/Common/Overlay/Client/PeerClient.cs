using System;
using System.Collections.Generic;
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
        private IEventLoopGroup worker_group = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public PeerClient()
        {
            this.worker_group = new EventLoopGroup(0);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private Task<IChannel> ConnectAsync(string host, int port, string remoteId, bool discoveryMode)
        {
            Logger.Info(
                string.Format("connect peer {0} {1} {2}", host, port, remoteId));

            //tronChannelInitializer.setPeerDiscoveryMode(discoveryMode);

            Bootstrap bootstrap = new Bootstrap();
            bootstrap.Group(this.worker_group);
            bootstrap.Channel<TcpSocketChannel>();

            bootstrap.Option(ChannelOption.SoKeepalive, true);
            bootstrap.Option(ChannelOption.MessageSizeEstimator, DefaultMessageSizeEstimator.Default);
            bootstrap.Option(ChannelOption.ConnectTimeout, new TimeSpan(Args.Instance.Node.ConnectionTimeout));
            bootstrap.RemoteAddress(host, port);

            bootstrap.Handler(new ActionChannelInitializer<TcpSocketChannel>(channel =>
            {
                Channel.Instance.Init();

                channel.Allocator.Buffer(256 * 1024);
                channel.Configuration.SetOption(ChannelOption.SoRcvbuf, 256 * 1024);
                channel.Configuration.SetOption(ChannelOption.SoBacklog, 1024);

            }));

            return bootstrap.ConnectAsync();
        }
        #endregion


        #region External Method
        public void Connect(String host, int port, String remoteId)
        {
            try
            {
                ChannelFuture f = connectAsync(host, port, remoteId, false);
                f.sync().channel().closeFuture().sync();
            }
            catch (Exception e)
            {
                logger
                    .info("PeerClient: Can't connect to " + host + ":" + port + " (" + e.getMessage() + ")");
            }
        }

        public Task<IChannel> ConnectAsync(NodeHandler handler, bool discovery_mode)
        {
            Node node = nodeHandler.getNode();
            return connectAsync(node.getHost(), node.getPort(), node.getHexId(), discoveryMode)
                .addListener((ChannelFutureListener)future => {
                if (!future.isSuccess())
                {
                    logger.warn("connect to {}:{} fail,cause:{}", node.getHost(), node.getPort(),
                        future.cause().getMessage());
                    nodeHandler.getNodeStatistics().nodeDisconnectedLocal(ReasonCode.CONNECT_FAIL);
                    nodeHandler.getNodeStatistics().notifyDisconnect();
                    future.channel().close();
                }
            });
        }

        public void Close()
        {
            this.worker_group.ShutdownGracefullyAsync();
        }
        #endregion
    }
}

