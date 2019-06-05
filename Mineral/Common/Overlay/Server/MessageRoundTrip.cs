using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Overlay.Server
{
    using Message = Mineral.Common.Overlay.Messages.Message;

    public class MessageRoundTrip
    {
        #region Field
        private readonly Message message;
        private long last_timestamp = 0;
        private long retry_time = 0;
        private bool is_answerd = false;
        #endregion


        #region Property
        public Message Message { get { return this.message; } }
        public long LastTimestamp { get { return this.last_timestamp; } set { this.last_timestamp = value; } }
        public long RetryTime { get { return this.retry_time; } set { this.retry_time = value; } }
        public bool IsAnswered { get { return this.is_answerd; } set { this.is_answerd = value; } }
        #endregion


        #region Constructor
        public MessageRoundTrip(Message message)
        {
            this.message = message;
            SaveTime();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Answer()
        {
            this.is_answerd = true;
        }

        public void IncreaseRetryTime()
        {
            ++this.retry_time;
        }

        public void SaveTime()
        {
            this.last_timestamp = DateTime.Now.Ticks;
        }

        public bool HasToRetry()
        {
            return 20000 < DateTime.Now.Ticks - this.last_timestamp;
        }
        #endregion
    }
}
