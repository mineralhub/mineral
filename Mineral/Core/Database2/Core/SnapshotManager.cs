using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mineral.Common.Storage;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Database2.Common;
using Mineral.Core.Exception;
using Mineral.Utils;

namespace Mineral.Core.Database2.Core
{
    public class SnapshotManager : IRevokingDatabase
    {
        #region Field
        private static readonly int DEFAULT_STACK_MAX_SIZE = 256;
        public static readonly int DEFAULT_MAX_FLUSH_COUNT = 500;
        public static readonly int DEFAULT_MIN_FLUSH_COUNT = 1;

        private List<RevokingDBWithCaching> databases = new List<RevokingDBWithCaching>();
        private Dictionary<string, Task> flush_service = new Dictionary<string, Task>();
        private int max_size = DEFAULT_STACK_MAX_SIZE;
        private int size = 0;
        private bool is_disable = true;
        private int active_session = 0;
        private bool is_unchecked = true;
        private volatile int max_flush_count = DEFAULT_MAX_FLUSH_COUNT;
        private volatile int flush_count = DEFAULT_MIN_FLUSH_COUNT;
        private CheckTempStore check_temp_store = new CheckTempStore();
        #endregion


        #region Property
        public int Size { get { return this.size; } }
        public int MaxSize
        {
            get
            {
                lock (this)
                {
                    return this.MaxSize;
                }
            }
            set
            {
                lock (this)
                {
                    this.MaxSize = value;
                }
            }
        }
        public int MaxFlushCount { get { return this.max_flush_count; } set { this.max_flush_count = value; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Advance()
        {
            this.databases.ForEach(db => db.SetHead(db.GetHead().Advance()));
            ++size;
        }

        private void Retreat()
        {
            this.databases.ForEach(db => db.SetHead(db.GetHead().Retreat()));
            ++size;
        }

        private void Refresh()
        {
            List<Task> tasks = new List<Task>();
            foreach (RevokingDBWithCaching db in this.databases)
            {
                if (this.flush_service.TryGetValue(db.DBName, out Task task))
                {
                    task.Start();
                    tasks.Add(task);
                }
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException e)
            {
                Logger.Error(e.Message);
            }
        }

        private void RefreshOne(RevokingDBWithCaching db)
        {
            if (Snapshot.IsRoot(db.GetHead()))
                return;

            List<ISnapshot> snapshots = new List<ISnapshot>();

            SnapshotRoot root = (SnapshotRoot)db.GetHead().GetRoot();
            ISnapshot next = root;
            for (int i = 0; i < this.flush_count; ++i)
            {
                next = next.GetNext();
                snapshots.Add(next);
            }

            root.Merge(snapshots);
            root.ResetSolidity();

            if (db.GetHead() == next)
            {
                db.SetHead(root);
            }
            else
            {
                next.GetNext().SetPrevious(root);
                root.SetNext(next.GetNext());
            }
        }

        private bool ShouldBeRefreshed()
        {
            return this.flush_count >= this.max_flush_count;
        }

        private byte[] SimpleEncode(string name)
        {
            byte[] bytes = name.GetBytes();
            byte[] length = new byte[]
            {
                (byte)(bytes.Length >> 24),
                (byte)(bytes.Length >> 16),
                (byte)(bytes.Length >> 8),
                (byte)(bytes.Length >> 0)
            };
            byte[] result = new byte[4 + bytes.Length];
            Array.Copy(length, 0, result, 0, 4);
            Array.Copy(bytes, 0, result, 0, bytes.Length);

            return result;
        }

        private string SimpleDecode(byte[] bytes)
        {
            byte[] length_bytes = new byte[4];
            Array.Copy(bytes, length_bytes, 4);
            int length = (length_bytes[0] << 24 |
                        (length_bytes[1] & 0xFF) << 16 |
                        (length_bytes[2] & 0xFF) << 8 |
                        (length_bytes[3] & 0xFF) << 0);
            byte[] value = ByteUtil.CopyRange(bytes, 4, 4 + length);
            return value.GetString();
        }

        private void CreateCheckPoint()
        {
            Dictionary<byte[], byte[]> batch = new Dictionary<byte[], byte[]>();
            foreach (RevokingDBWithCaching db in this.databases)
            {
                ISnapshot head = db.GetHead();
                if (Snapshot.IsRoot(head))
                    return;

                string db_name = db.DBName;
                ISnapshot next = head.GetRoot();
                for (int i = 0; i < this.flush_count; ++i)
                {
                    next = next.GetNext();
                    Snapshot snapshot = (Snapshot)next;
                    IBaseDB<byte[], byte[]> key_value_db = snapshot.DB;
                    foreach (KeyValuePair<byte[], byte[]> pair in key_value_db)
                    {
                        byte[] name = SimpleEncode(db_name);
                        byte[] key = new byte[name.Length + pair.Key.Length];
                        Array.Copy(name, 0, key, 0, name.Length);
                        Array.Copy(pair.Key, 0, key, name.Length, pair.Key.Length);
                        batch.Add(key, pair.Value);
                    }
                }
            }

            this.check_temp_store.DBSource.UpdateByBatch(batch, WriteOptionWrapper.GetInstance().Sync(Args.Instance.Storage.Sync));
        }

        private void DeleteCheckPoint()
        {
            Dictionary<byte[], byte[]> collection = new Dictionary<byte[], byte[]>();
            if (!(this.check_temp_store.DBSource.AllKeys().Count <= 0))
            {
                IEnumerator<KeyValuePair<byte[], byte[]>> it = this.check_temp_store.GetEnumerator();
                while (it.MoveNext())
                {
                    collection.Add(it.Current.Key, it.Current.Value);
                }
            }

            this.check_temp_store.DBSource.UpdateByBatch(collection, WriteOptionWrapper.GetInstance().Sync(Args.Instance.Storage.Sync));
        }
        #endregion


        #region External Method
        #region Interface Method
        public ISession BuildSession()
        {
            return BuildSeesion(false);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ISession BuildSeesion(bool force_enable)
        {
            if (this.is_disable && !force_enable)
                return new Session(this);

            bool disable_exit = this.is_disable && force_enable;
            if (force_enable)
                this.is_disable = false;

            if (this.size > this.max_size)
            {
                this.flush_count = this.flush_count + (this.size - this.max_size);
                UpdateSolidity(this.size - this.max_size);
                this.size = this.max_size;
                flush();
            }

            Advance();
            ++this.active_session;

            return new Session(this, disable_exit);
        }

        public void Add(IRevokingDB revoking_db)
        {
            RevokingDBWithCaching db = (RevokingDBWithCaching)revoking_db;
            this.databases.Add(db);
            flush_service.Add(db.DBName, new Task((() => RefreshOne(db))));
        }

        public void Merge()
        {
            if (this.active_session <= 0)
                throw new RevokingStoreIllegalStateException("active dialog has to be greater than 0");

            if (this.size < 2)
                return;

            this.databases.ForEach(db => db.GetHead().GetPrevious().Merge(db.GetHead()));
            Retreat();
            --this.active_session;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Revoke()
        {
            if (this.is_disable)
                return;

            if (this.active_session <= 0)
                throw new RevokingStoreIllegalStateException("active dialog has to be greater than 0");

            if (this.size <= 0)
                return;

            this.is_disable = true;
            try
            {
                Retreat();
            }
            finally
            {
                this.is_disable = false;
            }
            --this.active_session;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Commit()
        {
            if (this.active_session <= 0)
                throw new RevokingStoreIllegalStateException("active dialog has to be greater than 0");
            --this.active_session;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Pop()
        {
            if (this.active_session != 0)
                throw new RevokingStoreIllegalStateException("active dialog has to be equal 0");

            if (this.size <= 0)
                throw new RevokingStoreIllegalStateException("there is not snapshot to be popped");

            this.is_disable = true;
            try
            {
                Retreat();
            }
            finally
            {
                this.is_disable = false;
            }
        }

        public void FastPop()
        {
            Pop();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Enable()
        {
            this.is_disable = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Disable()
        {
            this.is_disable = true;
        }

        public void Check()
        {
            foreach (RevokingDBWithCaching db in this.databases)
            {
                if (!Snapshot.IsRoot(db.GetHead()))
                {
                    throw new IllegalStateException("first check");
                }
            }

            if (this.check_temp_store.DBSource.AllKeys().Count > 0)
            {
                Dictionary<string, RevokingDBWithCaching> dbs = this.databases.ToDictionary(db => db.DBName);

                Advance();

                IEnumerator<KeyValuePair<byte[], byte[]>> it = this.check_temp_store.GetEnumerator();
                while (it.MoveNext())
                {
                    string db = SimpleDecode(it.Current.Key);
                    if (!dbs.TryGetValue(db, out RevokingDBWithCaching revoking_db))
                        continue;

                    byte[] key = it.Current.Key;
                    byte[] value = it.Current.Value;
                    byte[] db_bytes = db.GetBytes();
                    byte[] real_key = ByteUtil.CopyRange(key, db_bytes.Length + 4, key.Length);
                    byte[] real_value = value.Length == 1 ? null : ByteUtil.CopyRange(value, 1, value.Length);

                    if (real_value != null)
                        revoking_db.GetHead().Put(real_key, real_value);
                    else
                        revoking_db.GetHead().Remove(real_key);
                }

                this.databases.ForEach(db => db.GetHead().GetRoot().Merge(db.GetHead()));
                Retreat();
            }

            this.is_unchecked = false;
        }

        public void Shutdown()
        {
            Logger.Info("begin to pop revoking db");
            Logger.Info(string.Format("before revoking db size : {0}", this.size));

            try
            {
                while (ShouldBeRefreshed())
                {
                    Logger.Info("waiting db flush done");
                    Thread.Sleep(10);
                }
            }
            catch (System.Exception e)
            {
                Logger.Info(e.Message);
                Thread.CurrentThread.Interrupt();
            }

            this.check_temp_store.DBSource.Close();
            Logger.Info("end to pop revoking db");
        }

        public void SetMode(bool mode)
        {
            this.databases.ForEach(db => db.SetMode(mode));
        }
        #endregion

        public void UpdateSolidity(int hops)
        {
            for (int i = 0; i < hops; i++)
            {
                foreach  (RevokingDBWithCaching db in this.databases)
                {
                    db.GetHead().UpdateSolidity();
                }
            }
        }

        public void flush()
        {
            if (this.is_unchecked)
                return;

            if (ShouldBeRefreshed())
            {
                long start = Helper.CurrentTimeMillis();
                DeleteCheckPoint();
                CreateCheckPoint();
                long end = Helper.CurrentTimeMillis();
                Refresh();
                this.flush_count = 0;
                Logger.Info(
                    string.Format("flush cost:{0}, create checkpoint cost:{1}, refresh cost:{2}",
                                Helper.CurrentTimeMillis() - start,
                                end - start,
                                Helper.CurrentTimeMillis() - end
                ));
            }
        }

        #endregion
    }
}
