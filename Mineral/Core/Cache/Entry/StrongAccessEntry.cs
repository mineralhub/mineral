using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class StrongAccessEntry<TKey, TValue> : StrongEntry<TKey, TValue>
    {
        #region Field
        private long access_time = long.MaxValue;
        private IReferenceEntry<TKey, TValue> prev_access = new NullEntry<TKey, TValue>();
        private IReferenceEntry<TKey, TValue> next_access = new NullEntry<TKey, TValue>();
        #endregion


        #region Property
        public override long AccessTime
        {
            get { return this.access_time; }
            set { this.access_time = value; }
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
        #endregion


        #region Constructor
        public StrongAccessEntry(TKey key, int hash, IReferenceEntry<TKey, TValue> next)
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
