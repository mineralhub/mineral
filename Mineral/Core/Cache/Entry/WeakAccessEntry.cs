using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class WeakAccessEntry<TKey, TValue> : WeakEntry<TKey, TValue>
        where TKey : class
    {
        #region Field
        private long access_time = long.MaxValue;
        private IReferenceEntry<TKey, TValue> prev_access = new NullEntry<TKey, TValue>.Instance;
        private IReferenceEntry<TKey, TValue> next_access = new NullEntry<TKey, TValue>.Instance;
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


        #region Contructor
        public WeakAccessEntry(Queue<TKey> queue, TKey key, int hash, IReferenceEntry<TKey, TValue> next)
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
