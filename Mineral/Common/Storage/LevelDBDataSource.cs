using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using LevelDB;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;

namespace Mineral.Common.Storage
{
    public class LevelDBDataSource : IDBSourceInter<byte[]>, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        #region Field
        private string database_name;
        private string parent = "";
        private DB db;
        private object lock_wirte = new object();
        private object lock_read = new object();
        #endregion


        #region Property
        public string DataBaseName
        {
            get { return this.database_name; }
            set { this.parent = value; }
        }

        public string DataBasePath
        {
            get
            {
                string result = parent;
                if (!this.parent.Last().Equals(Path.DirectorySeparatorChar))
                {
                    result += Path.DirectorySeparatorChar;
                }
                result += this.database_name;

                return result;
            }
        }
        public bool IsAlive { get; set; } = false;
        #endregion


        #region Constructor
        public LevelDBDataSource(string parent, string name)
        {
            this.database_name = name;
            this.parent = parent.IsNullOrEmpty() ?
                Args.Instance.Storage.Directory : parent + Path.DirectorySeparatorChar + Args.Instance.Storage.Directory;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            if (!IsAlive)
            {
                try
                {
                    Monitor.Enter(this.lock_wirte);

                    Options options = Args.Instance.Storage.GetOptionsByDbName(DataBaseName);
                    this.db = new DB(options, DataBasePath);
                    IsAlive = this.db != null ? true : false;
                }
                catch (System.Exception e)
                {
                    IsAlive = false;
                    Logger.Error("Can't initialize database source", e);
                    throw e;
                }
                finally
                {
                    Monitor.Exit(this.lock_wirte);
                }
            }
        }

        public void Close()
        {
            Monitor.Enter(this.lock_wirte);
            this.db.Dispose();
            this.db = null;
            IsAlive = false;
            Monitor.Exit(this.lock_wirte);
        }

        public void Reset()
        {
            Close();
            FileUtils.RecursiveDelete(DataBasePath);
            Init();
        }

        public HashSet<byte[]> AllKeys()
        {
            Monitor.Enter(this.lock_read);
            HashSet<byte[]> result = new HashSet<byte[]>();

            try
            {
                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    for (it.SeekToFirst(); it.IsValid(); it.Next())
                    {
                        result.Add(it.Key());
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public HashSet<byte[]> AllValue()
        {
            Monitor.Enter(this.lock_read);
            HashSet<byte[]> result = new HashSet<byte[]>();

            try
            {
                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    for (it.SeekToFirst(); it.IsValid(); it.Next())
                    {
                        result.Add(it.Value());
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public void PutData(byte[] key, byte[] value)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Helper.IsNotNull(value, "Key must be not null.");

            Monitor.Enter(this.lock_read);
            try
            {
                PutData(key, value, new WriteOptions());
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }
        }

        public void PutData(byte[] key, byte[] value, WriteOptions options)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Helper.IsNotNull(value, "Key must be not null.");

            Monitor.Enter(this.lock_read);
            try
            {
                this.db.Put(key, value, options);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows)
        {
            UpdateByBatch(rows, new WriteOptions());
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows, WriteOptions options)
        {
            try
            {
                WriteBatch batch = new WriteBatch();
                foreach (KeyValuePair<byte[], byte[]> row in rows)
                {
                    batch.Put(row.Key, row.Value);
                }
                this.db.Write(batch, new WriteOptions());
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public byte[] GetData(byte[] key)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Monitor.Enter(this.lock_read);
            byte[] result = null;

            try
            {
                result = this.db.Get(key, new ReadOptions());
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public void DeleteData(byte[] key)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Monitor.Enter(this.lock_read);

            try
            {
                this.db.Delete(key, new WriteOptions());
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }
        }

        public void DeleteData(byte[] key, WriteOptions options)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Monitor.Enter(this.lock_read);

            try
            {
                this.db.Delete(key, options);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }
        }

        public long GetTotal()
        {
            Monitor.Enter(this.lock_read);
            long result = 0;

            try
            {
                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    for (it.SeekToFirst(); it.IsValid(); it.Next())
                    {
                        result++;
                    }
                }

            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }
            
            return result;
        }

        public HashSet<byte[]> GetLatestValues(long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            if (limit <= 0)
                return result;

            Monitor.Enter(this.lock_read);

            try
            {
                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    long i = 0;

                    it.SeekToLast();
                    if (it.IsValid())
                    {
                        result.Add(it.Value());
                        i++;
                    }

                    for (; it.IsValid() && i++ < limit; it.Prev())
                    {
                        result.Add(it.Value());
                    }
                }
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetValuesPrevious(byte[] key, long limit, int precision)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();

            if (limit <= 0 || key.Length < precision)
            {
                return result;
            }

            Monitor.Enter(this.lock_read);

            try
            {
                long i = 0;
                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    for (it.SeekToFirst(); it.IsValid() && i++ < limit; it.Next())
                    {
                        if (it.Key().Length >= precision)
                        {
                            if (ByteUtil.Compare(
                                    ArrayUtil.GetRange(key, 0, precision),
                                    ArrayUtil.GetRange(it.Key(), 0, precision))
                                    < 0)
                            {
                                break;
                            }
                            result.Add(it.Key(), it.Value());
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetPrevious(byte[] key, long limit)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();
            Monitor.Enter(this.lock_read);

            try
            {
                if (limit <= 0)
                    return result;

                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    long i = 0;

                    it.Seek(key);
                    it.Prev();
                    for (; it.IsValid() && i++ < limit; it.Prev())
                    {
                        result.Add(it.Key(), it.Value());
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetNext(byte[] key, long limit)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();
            Monitor.Enter(this.lock_read);

            try
            {
                if (limit <= 0)
                    return result;

                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    long i = 0;

                    it.Seek(key);
                    it.Next();
                    for (; it.IsValid() && i++ < limit; it.Next())
                    {
                        result.Add(it.Key(), it.Value());
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetAll()
        {
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();
            Monitor.Enter(this.lock_read);

            try
            {
                using (Iterator it = this.db.CreateIterator(new ReadOptions()))
                {
                    for (it.SeekToFirst(); it.IsValid(); it.Next())
                    {
                        result.Add(it.Key(), it.Value());
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                Monitor.Exit(this.lock_read);
            }

            return result;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return this.db.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<byte[], byte[]>>)GetEnumerator();
        }

        public bool Flush()
        {
            return false;
        }
        #endregion
    }
}
