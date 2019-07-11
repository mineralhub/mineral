using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mineral.Core.Capsule;
using Mineral.Utils;
using static Mineral.Core.Database.TransactionTrace;

namespace Mineral.Core.Database
{
    public class PendingManager : IDisposable
    {
        #region Field
        private DatabaseManager db_manager = null;
        private List<TransactionCapsule> transactions = new List<TransactionCapsule>();
        #endregion


        #region Property
        #endregion


        #region Contructor
        public PendingManager(DatabaseManager db_manager)
        {
            this.db_manager = db_manager;
            this.transactions.AddRange(db_manager.PendingTransactions);

            this.db_manager.PendingTransactions.Clear();
            this.db_manager.Session.Reset();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Dispose()
        {
            foreach (TransactionCapsule tx in this.transactions)
            {
                try
                {
                    if (tx.TransactionTrace != null
                        && tx.TransactionTrace.TimeResult.Equals(TimeResultType.Normal))
                    {
                        this.db_manager.RePushTransactions.Enqueue(tx);
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    Logger.Error(e.Message);
                    Thread.CurrentThread.Interrupt();
                }
            }

            this.transactions.Clear();

            foreach (TransactionCapsule tx in this.db_manager.PendingTransactions)
            {
                try
                {
                    if (tx.TransactionTrace != null
                        && tx.TransactionTrace.TimeResult.Equals(TimeResultType.Normal))
                    {
                        this.db_manager.RePushTransactions.Enqueue(tx);
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    Logger.Error(e.Message);
                    Thread.CurrentThread.Interrupt();
                }
            }

            this.db_manager.PopTransactions.Clear();
        }
    }
        #endregion
    }
}
