using Mineral.Core.Cache.Reference;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class WeakEntry<TKey, TValue> : IReferenceEntry<TKey, TValue>
        where TKey : class
    {
        #region Field
        private WeakReference<TKey> reference;
        private int hash;
        private IReferenceEntry<TKey, TValue> next;
        private volatile IValueReference<TKey, TValue> value_reference = new UnsetValueReference<TKey, TValue>();
        #endregion


        #region Property
        public virtual TKey Key
        {
            get
            {
                this.reference.TryGetTarget(out TKey key);
                return key;
            }
        }

        public virtual int Hash
        {
            get { return this.hash;}
        }

        public virtual IReferenceEntry<TKey, TValue> Next
        {
            get { return this.next; }
        }

        public virtual IValueReference<TKey, TValue> ValueReference
        {
            get { return this.value_reference; }
            set { this.value_reference = value; }
        }

        public virtual long AccessTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual long WriteTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> PrevInAccessQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> PrevInWriteQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> NextInAccessQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> NextInWriteQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion


        #region Constructor
        public WeakEntry(Queue<TKey> queue, TKey key, int hash, IReferenceEntry<TKey, TValue> next)
        {
            this.reference = new WeakReference<TKey>(key);
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
