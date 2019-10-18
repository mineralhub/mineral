using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Net.Peer;

namespace Mineral.Common.Overlay.Server
{
    public class PeerServer
    {
        #region Field
        private IChannel channel = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public async void Start(int port)
        {
            IEventLoopGroup boss_group = new MultithreadEventLoopGroup(1);
            IEventLoopGroup worker_group = new MultithreadEventLoopGroup(Args.Instance.Node.TcpNettyWorkThreadNum);

            try
            {
                ServerBootstrap bootstrap = new ServerBootstrap();

                bootstrap.Group(boss_group, worker_group);
                bootstrap.Channel<TcpServerSocketChannelEx>();
                bootstrap.Option(ChannelOption.SoKeepalive, true);
                bootstrap.Option(ChannelOption.MessageSizeEstimator, DefaultMessageSizeEstimator.Default);
                bootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(Args.Instance.Node.ConnectionTimeout));
                bootstrap.Handler(new LoggingHandler());
                bootstrap.ChildHandler(new NettyChannelInitializer("", false));

                Logger.Info("Tcp listener started, bind port : " + port);

                this.channel = await bootstrap.BindAsync(port);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message, e);
            }
            finally
            {
            }
        }

        public async void Close()
        {
            if (this.channel != null && this.channel.Active)
            {
                try
                {
                    Logger.Info("Closing TCP server...");
                    await this.channel.CloseAsync();
                }
                catch (Exception e)
                {
                    Logger.Warning("Closing TCP server failed." + e.Message);
                }
            }
        }
        #endregion
    }
}
