using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public class SnapshotRoot : AbstractSnapshot<byte[], byte[]>
    {
        #region Field
        private ISnapshot solidity = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public SnapshotRoot(string parent, string name)
        {
            try
            {
                this.db = (IBaseDB<byte[], byte[]>)new Common.LevelDB(parent, name);
            }
            catch (System.Exception e)
            {
                Logger.Error(e);
                throw e;
            }

            this.solidity = this;
        }

        public SnapshotRoot(string parent, string name, Type db_type)
        {
            try
            {
                if (db_type == typeof(Common.LevelDB))
                {
                    this.db = (IBaseDB<byte[], byte[]>)new Mineral.Core.Database2.Common.LevelDB(parent, name);
                }
                else if (db_type == typeof(TxCacheDB))
                {
                    this.db = new TxCacheDB();
                }
                else
                {
                    throw new ArgumentException("Not support database type");
                }
            }
            catch (System.Exception e)
            {
                Logger.Error(e);
                throw e;
            }

            this.solidity = this;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override void Close()
        {
            ((Flusher)this.db).Close();
        }

        public override byte[] Get(byte[] key)
        {
            return this.db.Get(key);
        }

        public override void Put(byte[] key, byte[] value)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Helper.IsNotNull(value, "Key must be not null.");

            this.db.Put(key, value);
        }

        public override void Remove(byte[] key)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            this.db.Remove(key);
        }

        public override ISnapshot GetRoot()
        {
            return this;
        }

        public override ISnapshot GetSolidity()
        {
            return this.solidity;
        }

        public override void Merge(ISnapshot snapshot)
        {
            Dictionary<WrappedByteArray, WrappedByteArray> batch = new Dictionary<WrappedByteArray, WrappedByteArray>();

            IEnumerator<KeyValuePair<Key, Value>> datas = ((Snapshot)snapshot).GetEnumerator();
            while (datas.MoveNext())
            {
                batch.Add(WrappedByteArray.Of(datas.Current.Key.Data), WrappedByteArray.Of(datas.Current.Value.Data));
            }
            ((Flusher)this.db).Flush(batch);
        }

        public void Merge(List<ISnapshot> snapshots)
        {
            Dictionary<WrappedByteArray, WrappedByteArray> batch = new Dictionary<WrappedByteArray, WrappedByteArray>();
            foreach (ISnapshot snapshot in snapshots)
            {
                Snapshot from = (Snapshot)snapshot;
                IEnumerator<KeyValuePair<Key, Value>> it = from.DB.GetEnumerator();
                while (it.MoveNext())
                {
                    batch.Add(WrappedByteArray.Of(it.Current.Key.Data), WrappedByteArray.Of(it.Current.Value.Data));
                }
            }

            ((Flusher)this.db).Flush(batch);
        }

        public override void Reset()
        {
            ((Flusher)this.db).Reset();
        }

        public override void ResetSolidity()
        {
            this.solidity = this;
        }

        public override ISnapshot Retreat()
        {
            return this;
        }

        public override void UpdateSolidity()
        {
            this.solidity = this.solidity.GetNext();
        }
        #endregion
    }
}
