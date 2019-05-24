using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mineral.Common.Stroage.LevelDB;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public class Snapshot : AbstractSnapshot<byte[], byte[]>
    {
        #region Field
        private ISnapshot root = null;
        #endregion


        #region Property
        protected ISnapshot Root { get { return this.root; } }
        #endregion


        #region Constructor
        public Snapshot(ISnapshot snapshot)
        {
            this.root = snapshot.GetRoot();
            this.previous = snapshot;
            snapshot.SetNext(this);
            this.db = new HashDB();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static bool IsRoot(ISnapshot snapshot)
        {
            return snapshot != null && typeof(Snapshot) == typeof(SnapshotRoot);
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return ((HashDB)(this.db)).GetEnumerator();
        }

        public override ISnapshot GetRoot()
        {
            return this.root;
        }

        public byte[] Get(ISnapshot head, byte[] key)
        {
            ISnapshot snapshot = head;

            Slice slice;
            while (IsRoot(snapshot))
            {
                slice = ((Snapshot)(snapshot)).db.Get(key);
                if (slice != default(Slice))
                {
                    return slice.ToArray();
                }
                snapshot = snapshot.GetPrevious();
            }
            return snapshot?.Get(key);
        }

        public override ISnapshot GetSolidity()
        {
            return this.root.GetSolidity();
        }

        public override ISnapshot Retreat()
        {
            return this.previous;
        }

        public override byte[] Get(byte[] key)
        {
            return Get(this, key);
        }

        public override void Put(byte[] key, byte[] value)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Helper.IsNotNull(value, "Key must be not null.");

            this.db.Put(key, value);
        }

        public override void Merge(ISnapshot snapshot)
        {
            HashDB hash_db = null;
            if (((Snapshot)snapshot).db is HashDB)
            {
                hash_db = (HashDB)((Snapshot)snapshot).db;
                IEnumerator<KeyValuePair<byte[], byte[]>> enumerator = hash_db.GetEnumerator();

                while (hash_db.GetEnumerator().MoveNext())
                    this.db.Put(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

        public override void Remove(byte[] key)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            this.db.Remove(key);
        }

        public override void Reset()
        {
            this.root.Reset();
        }

        public override void Close()
        {
            this.root.Close();
        }

        public override void ResetSolidity()
        {
            this.root.ResetSolidity();
        }

        public override void UpdateSolidity()
        {
            this.root.UpdateSolidity();
        }
        #endregion
    }
}
