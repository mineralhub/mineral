using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class StrongWriteEntry<TKey, TValue> : StrongEntry<TKey, TValue>
    {
        #region Field
        private long write_time = long.MaxValue;
        private IReferenceEntry<TKey, TValue> prev_write = new NullEntry<TKey, TValue>();
        private IReferenceEntry<TKey, TValue> next_write = new NullEntry<TKey, TValue>();
        #endregion


        #region Property
        public override long WriteTime
        {
            get { return this.write_time; }
            set { this.write_time = value; }
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


        #region Constructor
        public StrongWriteEntry(TKey key, int hash, IReferenceEntry<TKey, TValue> next)
            : base(key, hash, next)
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
