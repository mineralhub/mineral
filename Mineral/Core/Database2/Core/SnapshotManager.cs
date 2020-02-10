using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LevelDB;
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
        public class RefreshData
        {
            public int FlushCount { get; set; }
            public ManualResetEvent MRE { get; set; }
            public RevokingDBWithCaching DB { get; set; }
        }

        #region Field
        private static readonly int DEFAULT_STACK_MAX_SIZE = 256;
        public static readonly int DEFAULT_MAX_FLUSH_COUNT = 500;
        public static readonly int DEFAULT_MIN_FLUSH_COUNT = 1;

        private List<RevokingDBWithCaching> databases = new List<RevokingDBWithCaching>();
        private Dictionary<string, Task> flush_service = new Dictionary<string, Task>();
        private object locker = new object();
        private int max_size = DEFAULT_STACK_MAX_SIZE;
        private int size = 0;
        private bool is_disable = true;
        private int active_session = 0;
        private bool is_unchecked = true;
        private volatile int max_flush_count = DEFAULT_MAX_FLUSH_COUNT;
        private volatile int flush_count = 0;
        #endregion


        #region Property
        public int Size { get { return this.size; } }
        public int MaxSize
        {
            get
            {
                return this.max_size;
            }
            set
            {
                lock (this.locker)
                {
                    this.max_size = value;
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
            --size;
        }

        private void Refresh()
        {
            try
            {
                int i = 0;
                ManualResetEvent[] handles = new ManualResetEvent[this.databases.Count];

                foreach (RevokingDBWithCaching db in this.databases)
                {
                    handles[i] = new ManualResetEvent(false);
                    RefreshData data = new RefreshData()
                    {
                        FlushCount = this.flush_count,
                        MRE = handles[i],
                        DB = db
                    };

                    ThreadPool.QueueUserWorkItem((object state) =>
                    {
                        RefreshData item = (RefreshData)state;
                        try
                        {
                            RefreshOne(item);
                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                        finally
                        {
                            item.MRE.Set();
                        }
                    }, data);
                    i++;
                }

                WaitHandle.WaitAll(handles);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message, e);
            }
        }

        private static void RefreshOne(object state)
        {
            RefreshData data = (RefreshData)state;

            int flush_count = data.FlushCount;
            RevokingDBWithCaching db = data.DB;

            if (Snapshot.IsRoot(db.GetHead()))
                return;

            List<ISnapshot> snapshots = new List<ISnapshot>();

            SnapshotRoot root = (SnapshotRoot)db.GetHead().GetRoot();
            ISnapshot next = root;
            for (int i = 0; i < flush_count; ++i)
            {
                next = next.GetNext();
                snapshots.Add(next);
            }

            root.Merge(snapshots, data.DB.DBName);
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
            byte[] bytes = name.ToBytes();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] result = new byte[sizeof(int) + bytes.Length];

            Array.Copy(length, result, length.Length);
            Array.Copy(bytes, 0, result, length.Length, bytes.Length);

            return result;
        }

        private string SimpleDecode(byte[] bytes)
        {
            int length = BitConverter.ToInt32(bytes, 0);
            byte[] value = new byte[length];
            Array.Copy(bytes, 0, value, 0, length);

            return value.BytesToString();
        }

        private void CreateCheckPoint()
        {
            // Do not use compare
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
                    IBaseDB<Common.Key, Common.Value> key_value_db = snapshot.DB;
                    foreach (KeyValuePair<Common.Key, Common.Value> pair in key_value_db)
                    {
                        byte[] name = SimpleEncode(db_name);
                        byte[] key = new byte[name.Length + pair.Key.Data.Length];
                        Array.Copy(name, 0, key, 0, name.Length);
                        Array.Copy(pair.Key.Data, 0, key, name.Length, pair.Key.Data.Length);
                        batch.Add(key, pair.Value.Encode());
                    }
                }
            }

            // TODO : temp 계속 저장만 하는지 확인해야봐야함
            CheckTempStore.Instance.DBSource.UpdateByBatch(batch, new WriteOptions() { Sync = Args.Instance.Storage.Sync });
        }

        private void DeleteCheckPoint()
        {
            Dictionary<byte[], byte[]> collection = new Dictionary<byte[], byte[]>();
            foreach (var entry in CheckTempStore.Instance.DBSource)
            {
                collection.Add(entry.Key, entry.Value);
            }

            CheckTempStore.Instance.DBSource.UpdateByBatch(collection, new WriteOptions() { Sync = Args.Instance.Storage.Sync });
        }
        #endregion


        #region External Method
        #region Interface Method
        public ISession BuildSession()
        {
            return BuildSession(false);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ISession BuildSession(bool force_enable)
        {
            ISession session = null;

            using (Profiler.Measure("BuildSession"))
            {
                Profiler.PushFrame("Step-4-1");
                if (this.is_disable && !force_enable)
                    return new Session(this);

                Profiler.NextFrame("Step-4-2");
                bool disable_exit = this.is_disable && force_enable;
                if (force_enable)
                    this.is_disable = false;

                if (this.size > this.max_size)
                {
                    Profiler.NextFrame("Step-4-3");
                    this.flush_count = this.flush_count + (this.size - this.max_size);
                    UpdateSolidity(this.size - this.max_size);
                    Profiler.NextFrame("Step-4-4");
                    this.size = this.max_size;
                    Flush();
                }

                Profiler.NextFrame("Step-4-5");
                Advance();
                Profiler.NextFrame("Step-4-6");
                ++this.active_session;

                Profiler.NextFrame("Step-4-7");
                session = new Session(this, disable_exit);
                Profiler.PopFrame();
            }

            return session;
        }

        public void Add(IRevokingDB revoking_db)
        {
            RevokingDBWithCaching db = (RevokingDBWithCaching)revoking_db;
            this.databases.Add(db);
        }

        public void Merge()
        {
            if (this.active_session <= 0)
                throw new RevokingStoreIllegalStateException("active dialog has to be greater than 0");

            if (this.size < 2)
                return;

            using (Profiler.Measure("Snapshot-Merge"))
            {
                Profiler.PushFrame("Merge");
                this.databases.ForEach(db => db.GetHead().GetPrevious().Merge(db.GetHead()));
                Profiler.NextFrame("Retreat");
                Retreat();
                Profiler.NextFrame("active session");
                --this.active_session;
                Profiler.PopFrame();
            }
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

            if (CheckTempStore.Instance.DBSource.AllKeys().Count > 0)
            {
                Dictionary<string, RevokingDBWithCaching> dbs = this.databases.ToDictionary(db => db.DBName);

                Advance();

                foreach (var entry in CheckTempStore.Instance.DBSource)
                {
                    string db = SimpleDecode(entry.Key);
                    if (!dbs.TryGetValue(db, out RevokingDBWithCaching revoking_db))
                        continue;

                    byte[] key = entry.Key;
                    byte[] value = entry.Value;
                    byte[] db_bytes = db.ToBytes();
                    byte[] real_key = ArrayUtil.CopyRange(key, db_bytes.Length + 4, key.Length);
                    byte[] real_value = value.Length == 1 ? null : ArrayUtil.CopyRange(value, 1, value.Length);

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

            CheckTempStore.Instance.DBSource.Close();
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

        public void Flush()
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
