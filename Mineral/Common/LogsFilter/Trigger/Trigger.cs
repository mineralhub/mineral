using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.LogsFilter.Trigger
{
    public class Trigger
    {
        #region Field
        public static readonly int BLOCK_TRIGGER = 0;
        public static readonly int TRANSACTION_TRIGGER = 1;
        public static readonly int CONTRACTLOG_TRIGGER = 2;
        public static readonly int CONTRACTEVENT_TRIGGER = 3;

        public static readonly string BLOCK_TRIGGER_NAME = "block_trigger";
        public static readonly string TRANSACTION_TRIGGER_NAME = "transaction_trigger";
        public static readonly string CONTRACT_LOG_TRIGGER_NAME = "contract_log_trigger";
        public static readonly string CONTRACT_EVENT_TRIGGER_NAME = "contract_event_trigger";

        private string trigger_name = "";
        protected long timestamp = 0;
        #endregion


        #region Property
        public long Timestamp
        {
            get { return this.timestamp; }
            set { this.timestamp = value; }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
