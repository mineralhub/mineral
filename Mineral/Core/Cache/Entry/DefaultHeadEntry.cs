using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class DefaultHeadEntry<TKey, TValue> : AbstractReferenceEntry<TKey, TValue>
    {
        #region Field
        private IReferenceEntry<TKey, TValue> prev_write;
        private IReferenceEntry<TKey, TValue> next_write;
        #endregion


        #region Property
        public override long WriteTime
        {
            get { return long.MaxValue; }
            set { }
        }

        public override IReferenceEntry<TKey, TValue> PrevInWriteQueue
        {
            get { return prev_write; }
            set { this.prev_write = value; }
        }

        public override IReferenceEntry<TKey, TValue> NextInWriteQueue
        {
            get { return next_write; }
            set { this.next_write = value; }
        }
        #endregion


        #region Contructor
        public DefaultHeadEntry()
        {
            this.next_write = this;
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
