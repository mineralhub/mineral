using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Common;
using Mineral.Utils;

namespace Mineral.Core.Database2.Core
{
    public class RevokingDBWithCaching : IRevokingDB
    {
        #region Field
        private Type db_type;
        private string db_name = "";
        private ISnapshot head = null;
        private ThreadLocal<bool> mode = new ThreadLocal<bool>();
        object lock_db = new object();
        #endregion


        #region Property
        public string DBName { get { return this.db_name; } }
        #endregion


        #region Constructor
        public RevokingDBWithCaching(string db_name, Type db_type)
        {
            this.db_name = db_name;
            this.db_type = db_type;
            this.head = new SnapshotRoot(Args.Instance.GetOutputDirectoryByDBName(this.db_name), this.db_name, this.db_type);
            this.mode.Value = true;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private ISnapshot Head()
        {
            if (this.mode == null || this.mode.Value == true)
            {
                return this.head;
            }
            else
            {
                return this.head.GetSolidity();
            }
        }
        #endregion


        #region External Method
        public ISnapshot GetHead()
        {
            ISnapshot snapshot = null;
            lock (lock_db)
            {
                snapshot = Head();
            }
            return snapshot;
        }

        public void SetHead(ISnapshot snapshot)
        {
            lock (lock_db)
            {
                this.head = snapshot;
            }
        }

        public void SetMode(bool mode)
        {
            this.mode.Value = mode;
        }

        public void Close()
        {
            lock (lock_db)
            {
                Head().Close();
            }
        }

        public void Reset()
        {
            lock (lock_db)
            {
                Head().Reset();
                Head().Close();
                this.head = new SnapshotRoot(Args.Instance.GetOutputDirectoryByDBName(this.db_name), this.db_name, this.db_type);
            }
        }

        public byte[] GetUnchecked(byte[] key)
        {
            byte[] value = null;
            lock (lock_db)
            {
                value = Head().Get(key);
            }

            return value;
        }

        public bool Contains(byte[] key)
        {
            return GetUnchecked(key) != null;
        }

        public void Put(byte[] key, byte[] value)
        {
            lock (lock_db)
            {
                Head().Put(key, value);
            }
        }

        public byte[] Get(byte[] key)
        {
            return GetUnchecked(key);
        }

        public void Delete(byte[] key)
        {
            lock (lock_db)
            {
                Head().Remove(key);
            }
        }

        public HashSet<byte[]> GetLatestValues(long limit)
        {
            return GetLatestValues(Head(), limit);
        }

        public HashSet<byte[]> GetLatestValues(ISnapshot head, long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            lock (lock_db)
            {
                if (limit <= 0)
                    return result;

                long temp = limit;
                ISnapshot snapshot = this.head;
                for (; limit > 0 && snapshot.GetPrevious() != null; snapshot = snapshot.GetPrevious())
                {
                    if (!((Snapshot)(snapshot)).DB.IsEmpty)
                    {
                        --temp;
                        IEnumerator<KeyValuePair<byte[], byte[]>> it = ((Snapshot)(snapshot)).GetEnumerator();
                        while (it.MoveNext())
                        {
                            result.Add(it.Current.Value);
                        }
                    }
                }

                if (snapshot.GetPrevious() == null && temp != 0)
                {
                    if (((SnapshotRoot)(head.GetRoot())).DB.GetType() == typeof(LevelDB))
                    {
                        HashSet<byte[]> values = (((LevelDB)((SnapshotRoot)snapshot).DB)).DB.GetLatestValues(temp);
                    }
                    else if (((SnapshotRoot)(head.GetRoot())).DB.GetType() == typeof(RocksDB))
                    {
                        HashSet<byte[]> values = (((RocksDB)((SnapshotRoot)snapshot).DB)).DB.GetLatestValues(temp);
                    }
                }
            }

            return result;
        }

        public HashSet<byte[]> GetValuesNext(byte[] key, long limit)
        {
            return GetValuesNext(Head(), key, limit);
        }

        public HashSet<byte[]> GetValuesNext(ISnapshot snapshot, byte[] key, long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            if (limit <= 0)
                return result;

            Dictionary<byte[], byte[]> collection = new Dictionary<byte[], byte[]>();
            if (snapshot.GetPrevious() != null)
            {
                ((Snapshot)(snapshot)).Collect(collection);
            }

            Dictionary<byte[], byte[]> db_dictonary = null;
            if (((SnapshotRoot)snapshot.GetRoot()).DB.GetType().Equals(typeof(LevelDB)))
            {
                db_dictonary = new Dictionary<byte[], byte[]>((((LevelDB)((SnapshotRoot)snapshot.GetRoot()).DB).DB.GetNext(key, limit)));
            }
            else if (((SnapshotRoot)snapshot.GetRoot()).DB.GetType().Equals(typeof(RocksDB)))
            {
                db_dictonary = new Dictionary<byte[], byte[]>((((RocksDB)((SnapshotRoot)snapshot.GetRoot()).DB).DB.GetNext(key, limit)));
            }

            foreach (KeyValuePair<byte[], byte[]> pair in collection)
            {
                db_dictonary.Add(pair.Key, pair.Value);
            }


            return db_dictonary.Keys
                    .OrderBy(x => x, new UnsignedByteArrayCompare())
                    .Where(z => ByteUtil.Compare(z, key) >= 0)
                    .Take((int)limit)
                    .ToHashSet();
        }

        public HashSet<byte[]> GetValuesPrevious(byte[] key, long limit)
        {
            Dictionary<byte[], byte[]> collection = new Dictionary<byte[], byte[]>();

            if (this.head.GetPrevious() != null)
            {
                ((Snapshot)head).Collect(collection);
            }

            int precision = sizeof(long) / sizeof(byte);
            HashSet<byte[]> result = new HashSet<byte[]>();

            foreach (byte[] array in collection.Keys)
            {
                if (ByteUtil.Compare(ByteUtil.GetRange(array, 0, precision), key) <= 0)
                {
                    result.Add(array);
                    limit--;
                }
            }

            if (limit <= 0)
                return result;

            List<byte[]> list = null;
            if (((SnapshotRoot)this.head.GetRoot()).DB.GetType().Equals(typeof(LevelDB)))
            {
                list = ((LevelDB)((SnapshotRoot)this.head.GetRoot()).DB).DB.GetPrevious(key, limit, precision).Values.ToList();
            }
            else if (((SnapshotRoot)this.head.GetRoot()).DB.GetType().Equals(typeof(RocksDB)))
            {
                list = ((RocksDB)((SnapshotRoot)this.head.GetRoot()).DB).DB.GetPrevious(key, limit, precision).Values.ToList();
            }

            foreach (byte[] array in list)
            {
                result.Add(array);
            }

            return result.Take((int)limit).ToHashSet();
        }

        public Dictionary<byte[], byte[]> GetAllValues()
        {
            Dictionary<byte[], byte[]> collection = new Dictionary<byte[], byte[]>();
            if (this.head.GetPrevious() != null)
            {
                ((Snapshot)this.head).Collect(collection);
            }
            
            Dictionary<byte[], byte[]> result = ((LevelDB)((SnapshotRoot)this.head.GetRoot()).DB).DB.GetAll();
            foreach (KeyValuePair<byte[], byte[]> pair in collection)
            {
                result.Add(pair.Key, pair.Value);
            }

            return result;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
