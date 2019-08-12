using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class StrongEntry<TKey, TValue> : AbstractReferenceEntry<TKey, TValue>
    {
        #region Field
        private TKey key;
        private int hash;
        private IReferenceEntry<TKey, TValue> next;
        volatile IValueReference<TKey, TValue> value_reference = new UnsetValueReference<TKey, TValue>();
        #endregion


        #region Property
        public override TKey Key
        {
            get { return this.key; }
        }

        public override int Hash
        {
            get { return this.hash; }
        }

        public override IReferenceEntry<TKey, TValue> Next
        {
            get { return this.next; }
        }

        public override IValueReference<TKey, TValue> ValueReference
        {
            get { return this.value_reference; }
            set { this.value_reference = value; }
        }
        #endregion


        #region Constructor
        public StrongEntry(TKey key, int hash, IReferenceEntry<TKey, TValue> next)
        {
            this.key = key;
            this.hash = hash;
            this.next = next;
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
