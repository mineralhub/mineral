using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Stroage.LevelDB;
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
        public SnapshotRoot(string parent, string name, Type db)
        {
            try
            {
                if (db.Equals(typeof(LevelDB)) || db.Equals(typeof(RocksDB)))
                {
                    this.db = (IBaseDB<byte[], byte[]>)Activator.CreateInstance(db, parent, name);
                }
                else
                {
                    this.db = (IBaseDB<byte[], byte[]>)Activator.CreateInstance(db);
                }
            }
            catch
            {
                throw new ArgumentException();
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
            throw new NotImplementedException();
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

        public override ISnapshot GetSolidity()
        {
            throw new NotImplementedException();
        }

        public override void Merge(ISnapshot snapshot)
        {
            //Dictionary<Slice, Slice> batch = ((Snapshot)snapshot).
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override void ResetSolidity()
        {
            throw new NotImplementedException();
        }

        public override ISnapshot Retreat()
        {
            throw new NotImplementedException();
        }

        public override void UpdateSolidity()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
