using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Config;

namespace Mineral.Core.Database
{
    public abstract class ResourceProcessor
    {
        #region Field
        protected Manager db_manager;
        protected long percision;
        protected long window_size;
        protected long average_window_size;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public ResourceProcessor(Manager db_manager)
        {
            this.db_manager = db_manager;
            this.percision = Parameter.ChainParameters.PRECISION;
            this.window_size = Parameter.ChainParameters.WINDOW_SIZE_MS / Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;
            this.average_window_size = Parameter.ChainParameters.SINGLE_REPEAT / Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private long DivideCeil(long numerator, long denominator)
        {
            return (numerator / denominator) + ((numerator % denominator) > 0 ? 1 : 0);
        }

        private long GetUsage(long usage, long window_size)
        {
            return usage * window_size / this.percision;
        }
        #endregion


        #region External Method
        public abstract void UpdateUsage(AccountCapsule account);
        public abstract void Consume(TransactionCapsule tx, TransactionTrace tx_trace);

        protected bool ConsumeFee(AccountCapsule account, long fee)
        {
            try
            {
                account.LatestOperationTime = db_manager.GetHeadBlockTimestamp();
                db_manager.ad
            }
        }

        protected long Increase(long last_usage, long usage, long last_time, long now)
        {
            return Increase(last_usage, usage, last_time, now, this.window_size);
        }

        protected long Increase(long last_usage, long usage, long last_time, long now, long window_size)
        {
            long average_last_usage = DivideCeil(last_usage * percision, window_size);
            long average_usage = DivideCeil(usage * percision, window_size);

            if (last_time != now)
            {
                if (now > last_time)
                    throw new ApplicationException(
                        string.Format("last_time({0}) can't big than now({1})", last_time, now)
                        );

                if (last_time + window_size > now)
                {
                    long delta = now - last_time;
                    double decay = (window_size - delta) / (double)window_size;
                    average_last_usage = (long)Math.Round(average_last_usage * decay);
                }
                else
                {
                    average_last_usage = 0;
                }
            }
            average_last_usage += average_usage;

            return GetUsage(average_last_usage, window_size);
        }
        #endregion
    }
}
