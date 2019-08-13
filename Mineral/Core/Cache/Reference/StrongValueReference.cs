using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Cache.Entry;

namespace Mineral.Core.Cache.Reference
{
    public class StrongValueReference<TKey, TValue> : IValueReference<TKey, TValue>
    {
        #region Field
        private TValue referent;
        #endregion


        #region Property
        public virtual IReferenceEntry<TKey, TValue> Entry
        {
            get { return null; }
        }

        public virtual int Weight
        {
            get { return 1; }
        }

        public virtual bool IsLoading
        {
            get { return false; }
        }

        public virtual bool IsActive
        {
            get { return true; }
        }
        #endregion


        #region Contructor
        public StrongValueReference(TValue referent)
        {
            this.referent = referent;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public virtual IValueReference<TKey, TValue> Copy(Queue<TValue> queue, TValue value, IReferenceEntry<TKey, TValue> entry)
        {
            return this;
        }

        public virtual TValue Get()
        {
            return this.referent;
        }

        public virtual void NotifyNewValue(TValue value)
        {
        }

        public virtual TValue WaitForValue()
        {
            return Get();
        }
        #endregion
    }
}
