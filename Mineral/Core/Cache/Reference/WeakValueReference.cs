using Mineral.Core.Cache.Entry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Reference
{
    public class WeakValueReference<TKey, TValue> : IValueReference<TKey, TValue>
        where TValue : class
    {
        #region Field
        private WeakReference<TValue> reference;
        private IReferenceEntry<TKey, TValue> entry;
        #endregion


        #region Property
        public IReferenceEntry<TKey, TValue> Entry
        {
            get { return this.entry; }
        }

        public int Weight
        {
            get { return 1; }
        }

        public bool IsLoading
        {
            get { return false; }
        }

        public bool IsActive
        {
            get { return true; }
        }
        #endregion


        #region Contructor
        public WeakValueReference(Queue<TValue> queue, TValue referent, IReferenceEntry<TKey, TValue> entry)
        {
            this.reference = new WeakReference<TValue>(referent);
            this.entry = entry;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public IValueReference<TKey, TValue> Copy(Queue<TValue> queue, TValue value, IReferenceEntry<TKey, TValue> entry)
        {
            return new WeakValueReference<TKey, TValue>(queue, value, entry);
        }

        public TValue Get()
        {
            throw new NotImplementedException();
        }

        public void NotifyNewValue(TValue value)
        {
        }

        public TValue WaitForValue()
        {
            return Get();
        }
        #endregion
    }
}
