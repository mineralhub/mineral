﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public abstract class AbstractSnapshot<T, V> : ISnapshot
    {
        #region Field
        protected ISnapshot previous;
        protected WeakReference<ISnapshot> next = null;
        protected IBaseDB<T, V> db { get; set; }
        #endregion


        #region Property
        public IBaseDB<T, V> DB { get { return this.db; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public ISnapshot Advance()
        {
            return new Snapshot((ISnapshot)this);
        }

        public void SetPrevious(ISnapshot snapshot)
        {
            this.previous = snapshot;
        }

        public void SetNext(ISnapshot next)
        {
            this.next = new WeakReference<ISnapshot>(next);
        }

        public ISnapshot GetPrevious()
        {
            return this.previous;
        }

        public ISnapshot GetNext()
        {
            ISnapshot result = null;
            if (this.next != null)
            {
                this.next.TryGetTarget(out result);
            }

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Abstract - ISnapshot
        public abstract ISnapshot GetRoot();
        public abstract ISnapshot GetSolidity();
        public abstract ISnapshot Retreat();
        public abstract byte[] Get(byte[] key);
        public abstract void Put(byte[] key, byte[] value);
        public abstract void Merge(ISnapshot snapshot);
        public abstract void Remove(byte[] key);
        public abstract void Reset();
        public abstract void ResetSolidity();
        public abstract void UpdateSolidity();
        public abstract void Close();
        public abstract IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator();
        #endregion
        #endregion
    }
}
