using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mineral.Core.Database2.Common;
using Mineral.Utils;

namespace Mineral.Core.Database2.Core
{
    public class Snapshot : AbstractSnapshot<Key, Value>
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
            return snapshot != null && snapshot.GetType() == typeof(SnapshotRoot);
        }

        public static bool IsImplement(ISnapshot snapshot)
        {
            return snapshot != null && snapshot.GetType() == typeof(Snapshot);
        }

        public override ISnapshot GetRoot()
        {
            return this.root;
        }

        public byte[] Get(ISnapshot head, byte[] key)
        {
            ISnapshot snapshot = head;

            Value result = null;
            while (IsImplement(snapshot))
            {
                result = ((Snapshot)(snapshot)).db.Get(Key.Of(key));
                if (result != null)
                {
                    return result.Data;
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

            this.db.Put(Key.CopyOf(key), Value.CopyOf(Value.Operator.PUT, value));
        }

        public override void Merge(ISnapshot snapshot)
        {
            Snapshot from = (Snapshot)snapshot;

            foreach (KeyValuePair<Key, Value> pair in from.db)
            {
                this.db.Put(pair.Key, pair.Value);
            }
        }

        public override void Remove(byte[] key)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            this.db.Put(Key.Of(key), Value.Of(Value.Operator.DELETE, null));
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

        public void Collect(Dictionary<WrappedByteArray, WrappedByteArray> collect)
        {
            ISnapshot next = GetRoot().GetNext();
            while (next != null)
            {
                foreach (var data in ((Snapshot)next).DB)
                {
                    collect.Put(WrappedByteArray.Of(data.Key.Data), WrappedByteArray.Of(data.Value.Data));
                }
                next = next.GetNext();
            }
        }

        public override IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            Dictionary<WrappedByteArray, WrappedByteArray> all =
                new Dictionary<WrappedByteArray, WrappedByteArray>(new WrapperdByteArrayEqualComparer());

            Collect(all);
            all = all.Where(item => item.Value != null && item.Value.Data != null).ToDictionary(p => p.Key, p => p.Value);

            return Enumerable.Concat(
                Enumerable.Select(all, val => new KeyValuePair<byte[], byte[]>(val.Key.Data, val.Value.Data)),
                Enumerable.Where(GetRoot(), val => !all.Keys.Contains(WrappedByteArray.Of(val.Key)))
                ).GetEnumerator();
        }
        #endregion
    }
}
