using Mineral.Core.Cache.Entry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Reference
{
    public class UnsetValueReference<TKey, TValue> : IValueReference<TKey, TValue>
    {
        #region Field
        #endregion


        #region Property
        public IReferenceEntry<TKey, TValue> Entry
        {
            get { return null; }
        }

        public int Weight
        {
            get { return 0; }
        }

        public bool IsLoading
        {
            get { return false; }
        }

        public bool IsActive
        {
            get { return false; }
        }

        public IValueReference<TKey, TValue> Copy(Queue<TValue> queue, TValue value, IReferenceEntry<TKey, TValue> entry)
        {
            return this;
        }

        public TValue Get()
        {
            return default(TValue);
        }

        public void NotifyNewValue(TValue value)
        {
        }

        public TValue WaitForValue()
        {
            return default(TValue);
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
