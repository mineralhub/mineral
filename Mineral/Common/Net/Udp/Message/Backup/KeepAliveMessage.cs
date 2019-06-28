using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Overlay.Discover.Node;

namespace Mineral.Common.Net.Udp.Message.Backup
{
    public class KeepAliveMessage : Message
    {
        #region Field
        private Protocol.BackupMessage message = null;
        #endregion


        #region Property
        public bool Flag
        {
            get { return this.message.Flag; }
        }

        public int Priority
        {
            get { return this.message.Priority; }
        }

        public override Node From
        {
            get { return null; }
        }

        public override long Timestamp
        {
            get { return 0; }
        }
        #endregion


        #region Contructor
        public KeepAliveMessage(byte[] data)
            : base(UdpMessageType.BACKUP_KEEP_ALIVE, data)
        {
            try
            {
                this.message = Protocol.BackupMessage.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public KeepAliveMessage(bool flag, int priority)
            : base(UdpMessageType.BACKUP_KEEP_ALIVE, null)
        {
            this.message = new Protocol.BackupMessage();
            this.message.Flag = flag;
            this.message.Priority = priority;
            this.data = this.message.ToByteArray();
        }

        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
