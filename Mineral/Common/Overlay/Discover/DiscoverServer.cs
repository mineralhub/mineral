using DotNetty.Codecs.Protobuf;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Mineral.Common.Net.Udp.Handler;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core;
using Mineral.Core.Config.Arguments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral.Common.Overlay.Discover
{
    public class DiscoverServer
    {
        #region Field
        private IChannel channel = null;
        private DiscoverExecutor discover_executor = null;
        private volatile bool is_shutdown = false;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            if (Args.Instance.Node.Discovery.Enable == true && !Args.Instance.IsFastForward)
            {
                if (Args.Instance.Node.ListenPort == 0)
                {
                    Logger.Error("Discovery can't be started while listen port == 0");
                }
                else
                {
                    Task.Run(() =>
                    {
                        Start();
                    });
                }
            }
        }

        public async void Start()
        {
            int port = Args.Instance.Node.ListenPort;
            IEventLoopGroup group = new MultithreadEventLoopGroup(Args.Instance.Node.UdpNettyWorkThreadNum);
            try
            {
                this.discover_executor = new DiscoverExecutor(Manager.Instance.NodeManager);
                this.discover_executor.Start();
                while (!this.is_shutdown)
                {
                    if (this.channel == null || !this.channel.Active)
                    {
                        Bootstrap bootstrap = new Bootstrap();
                        bootstrap.Group(group);
                        bootstrap.Channel<SocketDatagramChannel>();
                        bootstrap.Option(ChannelOption.SoBroadcast, true);
                        bootstrap.Handler(new ActionChannelInitializer<SocketDatagramChannel>(channel =>
                        {
                            //channel.Pipeline.AddLast(Manager.Instance.TrafficStats.UDP);
                            //channel.Pipeline.AddLast(new ProtobufVarint32LengthFieldPrepender());
                            //channel.Pipeline.AddLast(new ProtobufVarint32FrameDecoder());
                            channel.Pipeline.AddLast(new PacketDecoder());
                            MessageHandler message_handler = new MessageHandler(channel, Manager.Instance.NodeManager);
                            Manager.Instance.NodeManager.MessageSender = message_handler;
                            channel.Pipeline.AddLast(message_handler);
                        }));

                        this.channel = await bootstrap.BindAsync(port);

                        Logger.Info(
                            string.Format("Discovery server started, bind port {0}", port));
                    }
                    else
                    {
                        Thread.Sleep(10 * 1000);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Error(
                    string.Format("Start discovery server with port {0} failed.", port), e);
            }
            finally
            {
                await group.ShutdownGracefullyAsync();
            }
        }

        public void Close()
        {
            Logger.Info("Closing discovery server...");

            this.is_shutdown = true;
            if (this.channel != null)
            {
                try
                {
                    channel.CloseAsync().Wait(10 * 1000);
                }
                catch
                {
                    Logger.Info("Closing discovery server failed.");
                }
            }

            if (this.discover_executor != null)
            {
                try
                {
                    this.discover_executor.Close();
                }
                catch
                {
                    Logger.Info("Closing discovery executor failed.");
                }
            }
        }
        #endregion
    }
}
