using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Common;
using Mineral.Core.Exception;
using Mineral.Utils;

namespace Mineral.Core.Database2.Core
{
    public class RevokingDBWithCaching : IRevokingDB
    {
        #region Field
        private string db_name = "";
        private ISnapshot head = null;
        private ThreadLocal<bool?> mode = new ThreadLocal<bool?>();
        private object lock_db = new object();
        #endregion


        #region Property
        public string DBName
        {
            get { return this.db_name; }
        }

        public string Name
        {
            get { return GetType().Name; }
        }
        #endregion


        #region Constructor
        public RevokingDBWithCaching(string db_name, Type db_type)
        {
            this.db_name = db_name;
            this.head = new SnapshotRoot(Args.Instance.GetOutputDirectoryByDBName(db_name), db_name, db_type);
            this.mode.Value = true;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method

        #endregion


        #region External Method
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ISnapshot GetHead()
        {
            if (this.mode.Value == null || this.mode.Value == true)
            {
                return this.head;
            }
            else
            {
                return this.head.GetSolidity();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetHead(ISnapshot snapshot)
        {
            this.head = snapshot;
        }

        public void SetMode(bool mode)
        {
            this.mode.Value = mode;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Close()
        {
            GetHead().Close();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Reset()
        {
            GetHead().Close();
            this.head = new SnapshotRoot(Args.Instance.GetOutputDirectoryByDBName(this.db_name), this.db_name);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] GetUnchecked(byte[] key)
        {
            return GetHead().Get(key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(byte[] key)
        {
            return GetUnchecked(key) != null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Put(byte[] key, byte[] value)
        {
            GetHead().Put(key, value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Get(byte[] key)
        {
            byte[] result = GetUnchecked(key);
            if (result == null)
            {
                throw new ItemNotFoundException();
            }

            return GetUnchecked(key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Delete(byte[] key)
        {
            GetHead().Remove(key);
        }

        public HashSet<byte[]> GetLatestValues(long limit)
        {
            return GetLatestValues(GetHead(), limit);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public HashSet<byte[]> GetLatestValues(ISnapshot head, long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            lock (lock_db)
            {
                if (limit <= 0)
                    return result;

                long temp = limit;
                ISnapshot snapshot = this.head;
                for (; temp > 0 && snapshot.GetPrevious() != null; snapshot = snapshot.GetPrevious())
                {
                    if (!((Snapshot)(snapshot)).DB.IsEmpty)
                    {
                        --temp;
                        IEnumerator<KeyValuePair<Key, Value>> it = ((Snapshot)(snapshot)).GetEnumerator();
                        while (it.MoveNext())
                        {
                            result.Add(it.Current.Value.Data);
                        }
                    }
                }

                if (snapshot.GetPrevious() == null && temp != 0)
                {
                    foreach (var value in (((Common.LevelDB)((SnapshotRoot)snapshot).DB)).DB.GetLatestValues(temp))
                    {
                        result.Add(value);
                    }
                }
            }

            return result;
        }

        public HashSet<byte[]> GetValuesNext(byte[] key, long limit)
        {
            return GetValuesNext(GetHead(), key, limit);
        }

        public HashSet<byte[]> GetValuesNext(ISnapshot snapshot, byte[] key, long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            if (limit <= 0)
                return result;

            Dictionary<WrappedByteArray, WrappedByteArray> collection = new Dictionary<WrappedByteArray, WrappedByteArray>();
            if (snapshot.GetPrevious() != null)
            {
                ((Snapshot)(snapshot)).Collect(collection);
            }

            Dictionary<byte[], byte[]> db_dictonary =
                new Dictionary<byte[], byte[]>((((Common.LevelDB)((SnapshotRoot)snapshot.GetRoot()).DB).DB.GetNext(key, limit)));

            foreach (KeyValuePair<WrappedByteArray, WrappedByteArray> pair in collection)
            {
                db_dictonary.Add(pair.Key.Data, pair.Value.Data);
            }


            return db_dictonary
                    .OrderBy(x => x.Key, new UnsignedByteArrayCompare())
                    .Where(y => ByteUtil.Compare(y.Key, key) >= 0)
                    .Take((int)limit)
                    .Select(z => z.Value)
                    .ToHashSet();
        }

        public HashSet<byte[]> GetValuesPrevious(byte[] key, long limit)
        {
            Dictionary<WrappedByteArray, WrappedByteArray> collection = new Dictionary<WrappedByteArray, WrappedByteArray>();

            if (this.head.GetPrevious() != null)
            {
                ((Snapshot)head).Collect(collection);
            }

            int precision = sizeof(long) / sizeof(byte);
            HashSet<byte[]> result = new HashSet<byte[]>();

            foreach (WrappedByteArray array in collection.Keys)
            {
                if (ByteUtil.Compare(ArrayUtil.GetRange(array.Data, 0, precision), key) <= 0)
                {
                    result.Add(array.Data);
                    limit--;
                }
            }

            if (limit <= 0)
                return result;

            List<byte[]> list =
                ((Common.LevelDB)((SnapshotRoot)this.head.GetRoot()).DB).DB.GetValuesPrevious(key, limit, precision).Values.ToList(); ;

            foreach (byte[] array in list)
            {
                result.Add(array);
            }

            return result.Take((int)limit).ToHashSet();
        }

        public Dictionary<byte[], byte[]> GetAllValues()
        {
            Dictionary<WrappedByteArray, WrappedByteArray> collection = new Dictionary<WrappedByteArray, WrappedByteArray>();
            if (this.head.GetPrevious() != null)
            {
                ((Snapshot)this.head).Collect(collection);
            }

            Dictionary<byte[], byte[]> result = ((Common.LevelDB)((SnapshotRoot)this.head.GetRoot()).DB).DB.GetAll();
            foreach (KeyValuePair<WrappedByteArray, WrappedByteArray> pair in collection)
            {
                result.Add(pair.Key.Data, pair.Value.Data);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return this.head.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.head.GetEnumerator();
        }
        #endregion
    }
}
