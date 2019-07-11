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
        private bool listening = false;
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
        public void Start(int port)
        {
            IEventLoopGroup boos_group = new MultithreadEventLoopGroup(1);
            IEventLoopGroup worker_group = new MultithreadEventLoopGroup(Args.Instance.Node.TcpNettyWorkThreadNum);

            try
            {
                ServerBootstrap bootstrap = new ServerBootstrap();

                bootstrap.Group(boos_group, worker_group);
                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    .Option(ChannelOption.SoKeepalive, true)
                    .Option(ChannelOption.MessageSizeEstimator, DefaultMessageSizeEstimator.Default)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(Args.Instance.Node.ConnectionTimeout))
                    .Handler(new LoggingHandler())
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        channel.Pipeline.AddLast("readTimeoutHandler", new ReadTimeoutHandler(60));

                        //IChannelPipeline pipeline = channel.Pipeline;
                        //pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        //pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        //pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        //pipeline.AddLast("echo", new PeerServerHandler());
                    }));
            }
            finally
            {
            }
        }

        public void Close()
        {
            if (listening && channelFuture != null && channelFuture.channel().isOpen())
            {
                try
                {
                    logger.info("Closing TCP server...");
                    channelFuture.channel().close().sync();
                }
                catch (Exception e)
                {
                    logger.warn("Closing TCP server failed.", e);
                }
            }
        }
        #endregion
    }
}
