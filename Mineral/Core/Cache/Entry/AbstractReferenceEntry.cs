using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class AbstractReferenceEntry<TKey, TValue> : IReferenceEntry<TKey, TValue>
    {
        #region Field
        #endregion


        #region Property
        public virtual TKey Key => throw new NotImplementedException();
        public virtual int Hash => throw new NotImplementedException();
        public virtual IReferenceEntry<TKey, TValue> Next => throw new NotImplementedException();
        public virtual long AccessTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual long WriteTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IValueReference<TKey, TValue> ValueReference { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> PrevInAccessQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> PrevInWriteQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> NextInAccessQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IReferenceEntry<TKey, TValue> NextInWriteQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
