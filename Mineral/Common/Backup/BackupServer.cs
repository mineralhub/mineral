using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Codecs.Protobuf;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Mineral.Common.Net.Udp.Handler;
using Mineral.Common.Overlay.Server;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Backup
{
    public class BackupServer
    {
        #region Field
        private int port = (int)Args.Instance.Node.Backup.Port;
        private BackupManager backup_manager = null;
        private IChannel channel = null;
        private WireTrafficStats stats = new WireTrafficStats();
        private volatile bool is_shutdown = false;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public BackupServer(BackupManager backup_manager)
        {
            this.backup_manager = backup_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private async void Start()
        {
            IEventLoopGroup worker_group = new MultithreadEventLoopGroup(1);

            try
            {
                while (!this.is_shutdown)
                {
                    if (channel == null || !channel.Active)
                    {
                        Bootstrap bootstrap = new Bootstrap();
                        bootstrap.Group(worker_group);
                        bootstrap.Channel<SocketDatagramChannel>();
                        bootstrap.Handler(new ActionChannelInitializer<SocketDatagramChannel>(channel =>
                        {
                            channel.Pipeline.AddLast(this.stats.UDP);
                            channel.Pipeline.AddLast(new ProtobufVarint32LengthFieldPrepender());
                            channel.Pipeline.AddLast(new ProtobufVarint32FrameDecoder());
                            channel.Pipeline.AddLast(new PacketDecoder());
                            MessageHandler handler = new MessageHandler(channel, this.backup_manager);
                            this.backup_manager.MessageHandler = handler;
                            channel.Pipeline.AddLast(handler);
                        }));

                        this.channel = await bootstrap.BindAsync(port);

                        Logger.Info("Backup server started, bind port " + this.port);
                    }
                    else
                    {
                        Thread.Sleep(10 * 1000);
                    }
                }

                await this.channel.CloseAsync();
            }
            catch (System.Exception e)
            {
                Logger.Error(
                    string.Format("Start backup server with port {0} failed", this.port));
            }
            finally
            {
                await Task.WhenAll(worker_group.ShutdownGracefullyAsync());
            }

        }
        #endregion


        #region External Method
        public void InitServer()
        {
            if (port > 0 && Args.Instance.Node.Backup.Members.Count > 0)
            {
                new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        Start();
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error("Start backup server failed, " + e.Message);
                    }

                })).Start();
            }
        }

        public void Close()
        {
            Logger.Info("Closing backup server...");
            this.is_shutdown = true;

            if (this.channel != null)
            {
                try
                {
                    this.channel.CloseAsync().Wait(10 * 1000);
                }
                catch (Exception e)
                {
                    Logger.Warning("Closing backup server failed." +  e.Message);
                }
            }
        }
        #endregion
    }
}
