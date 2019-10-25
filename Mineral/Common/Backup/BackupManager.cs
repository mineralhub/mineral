using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Mineral.Common.Net.Udp;
using Mineral.Common.Net.Udp.Handler;
using Mineral.Common.Net.Udp.Message;
using Mineral.Common.Net.Udp.Message.Backup;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;
using static Mineral.Utils.ScheduledExecutorService;

namespace Mineral.Common.Backup
{
    public class BackupManager : IEventHandler
    {
        public enum BackupStatus
        {
            INIT,
            SLAVER,
            MASTER
        }

        #region Field
        private int priority = (int)Args.Instance.Node.Backup.Priority;
        private int port = (int)Args.Instance.Node.Backup.Port;
        private string local_ip = "";
        private BlockingCollection<string> members = new BlockingCollection<string>();
        private ScheduledExecutorHandle service_handler = null;
        private MessageHandler message_handler = null;
        private BackupStatus status = BackupStatus.MASTER;
        private long last_keep_alive_time = 0;
        private long keep_alive_timeout = 10_000;
        private volatile bool is_inited = false;
        #endregion


        #region Property
        public MessageHandler MessageHandler
        {
            private get { return this.message_handler; }
            set { this.message_handler = value; }
        }

        public BackupStatus Status
        {
            get { return this.status; }
            set
            {
                Logger.Info("Change backup status to " + value.ToString());
                this.status = value;
            }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void ChannelActivated()
        {
            if (this.is_inited)
                return;

            this.is_inited = true;

            try
            {
                IPAddress address = Dns.GetHostAddresses(Dns.GetHostName())
                                       .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                this.local_ip = address.ToString();
            }
            catch
            {
                Logger.Warning("Get local ip failed.");
            }

            foreach (String member in Args.Instance.Node.Backup.Members)
            {
                if (!this.local_ip.Equals(member))
                {
                    members.Add(member);
                }
            }

            Logger.Info(
                string.Format("Backup localIp:{0}, members: size= {1}, {2}",
                              this.local_ip,
                              members.Count,
                              members));

            this.status = BackupStatus.INIT;
            this.last_keep_alive_time = Helper.CurrentTimeMillis();

            this.service_handler = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    if (this.status != BackupStatus.MASTER
                        && Helper.CurrentTimeMillis() - this.last_keep_alive_time > this.keep_alive_timeout)
                    {
                        if (this.status == BackupStatus.SLAVER)
                        {
                            this.status = BackupStatus.INIT;
                            this.last_keep_alive_time = Helper.CurrentTimeMillis();
                        }
                        else
                        {
                            this.status = BackupStatus.MASTER;
                        }
                    }

                    if (this.status == BackupStatus.SLAVER)
                    {
                        return;
                    }

                    foreach (string member in this.members)
                    {
                        this.message_handler.Accept(
                            new UdpEvent(new KeepAliveMessage(this.status.Equals(BackupStatus.MASTER), priority),
                                         new IPEndPoint(IPAddress.Parse(member), port)));
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Error("Exception in send keep alive message : " + e.Message);
                }
            }, 1000, 1000);
        }

        public void HandlerEvent(UdpEvent udp_event)
        {
            IPEndPoint sender = udp_event.Address;

            if (udp_event.Message.Type != UdpMessageType.BACKUP_KEEP_ALIVE)
            {
                Logger.Warning(
                    string.Format("Receive not keep alive message from {0}, type {1}",
                                  sender.Address.ToString(),
                                  udp_event.Message.Type));
                return;
            }

            if (!this.members.Contains(sender.Address.ToString()))
            {
                Logger.Warning(
                    string.Format("Receive keep alive message from {0} is not my member.", sender.Address.ToString()));

                return;
            }

            this.last_keep_alive_time = Helper.CurrentTimeMillis();

            KeepAliveMessage message = (KeepAliveMessage)udp_event.Message;
            string ip = sender.Address.ToString();

            if (this.status == BackupStatus.INIT
                && (message.Flag || message.Priority > this.priority))
            {
                this.status = BackupStatus.SLAVER;
                return;
            }

            if (this.status == BackupStatus.MASTER && message.Flag)
            {
                if (message.Priority > priority)
                {
                    this.status = BackupStatus.SLAVER;
                }
                else if (message.Priority == priority && this.local_ip.CompareTo(ip) < 0)
                {
                    this.status = BackupStatus.SLAVER;
                }
            }
        }
        #endregion
    }
}
