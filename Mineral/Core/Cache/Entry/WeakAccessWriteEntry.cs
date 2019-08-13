using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class WeakAccessWriteEntry<TKey, TValue> : WeakEntry<TKey, TValue>
        where TKey : class
    {
        #region Field
        private long access_time = long.MaxValue;
        private long write_time = long.MaxValue;
        private IReferenceEntry<TKey, TValue> prev_access = NullEntry<TKey, TValue>.Instance;
        private IReferenceEntry<TKey, TValue> next_access = NullEntry<TKey, TValue>.Instance;
        private IReferenceEntry<TKey, TValue> prev_write = NullEntry<TKey, TValue>.Instance;
        private IReferenceEntry<TKey, TValue> next_write = NullEntry<TKey, TValue>.Instance;
        #endregion


        #region Property
        public override long AccessTime
        {
            get { return this.access_time; }
            set { this.access_time = value; }
        }

        public override long WriteTime
        {
            get { return this.write_time; }
            set { this.write_time = value; }
        }

        public override IReferenceEntry<TKey, TValue> PrevInAccessQueue
        {
            get { return this.prev_access; }
            set { this.prev_access = value; }
        }

        public override IReferenceEntry<TKey, TValue> NextInAccessQueue
        {
            get { return this.next_access; }
            set { this.next_access = value; }
        }

        public override IReferenceEntry<TKey, TValue> PrevInWriteQueue
        {
            get { return this.prev_write; }
            set { this.prev_write = value; }
        }

        public override IReferenceEntry<TKey, TValue> NextInWriteQueue
        {
            get { return this.next_write; }
            set { this.next_write = value; }
        }
        #endregion


        #region Contructor
        public WeakAccessWriteEntry(Queue<TKey> queue, TKey key, int hash, IReferenceEntry<TKey, TValue> next)
            : base(queue, key, hash, next)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
