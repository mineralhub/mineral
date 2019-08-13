using Mineral.Core.Cache.Entry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Queue
{
    public class AccessQueue<TKey, TValue>
    {
        #region Field
        private IReferenceEntry<TKey, TValue> head = new DefaultHeadEntry<TKey, TValue>();
        #endregion


        #region Property
        public bool IsEmpty
        {
            get { return this.head.NextInAccessQueue == this.head; }
        }

        public int Count
        {
            get
            {
                int size = 0;
                for (IReferenceEntry<TKey, TValue> entry = this.head.NextInAccessQueue; entry != head; entry = entry.NextInAccessQueue)
                {
                    size++;
                }

                return size;
            }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public bool Contains(IReferenceEntry<TKey, TValue> entry)
        {
            return entry.NextInAccessQueue != NullEntry<TKey, TValue>.Instance;
        }

        public void Enqueue(IReferenceEntry<TKey, TValue> entry)
        {
            ConnectAccessOrder(entry.PrevInAccessQueue, entry.NextInAccessQueue);
            ConnectAccessOrder(this.head.PrevInAccessQueue, entry);
            ConnectAccessOrder(entry, this.head);
        }

        public IReferenceEntry<TKey, TValue> Dequeue()
        {
            IReferenceEntry<TKey, TValue> next = this.head.NextInAccessQueue;
            if (next.Equals(this.head))
                return null;

            Remove(next);

            return next;
        }

        public IReferenceEntry<TKey, TValue> Peek()
        {
            IReferenceEntry<TKey, TValue> next = this.head.NextInAccessQueue;
            return (next.Equals(head)) ? null : next;
        }

        public bool Remove(IReferenceEntry<TKey, TValue> entry)
        {
            IReferenceEntry<TKey, TValue> prev = entry.PrevInAccessQueue;
            IReferenceEntry<TKey, TValue> next = entry.NextInAccessQueue;
            ConnectAccessOrder(prev, next);
            NullifyAccessOrder(entry);

            return next != NullEntry<TKey, TValue>.Instance;
        }

        public void Clear()
        {
            IReferenceEntry<TKey, TValue> entry = this.head.NextInAccessQueue;

            while (entry != this.head)
            {
                IReferenceEntry<TKey, TValue> next = entry.NextInAccessQueue;
                NullifyAccessOrder(entry);
                entry = next;
            }
        }

        public static void ConnectAccessOrder(IReferenceEntry<TKey, TValue> prev, IReferenceEntry<TKey, TValue> next)
        {
            prev.NextInAccessQueue = next;
            next.PrevInAccessQueue = prev;
        }

        public static void NullifyAccessOrder(IReferenceEntry<TKey, TValue> nulled)
        {
            IReferenceEntry<TKey, TValue> null_entry = NullEntry<TKey, TValue>.Instance;
            nulled.NextInAccessQueue = null_entry;
            nulled.PrevInAccessQueue = null_entry;
        }
        #endregion
    }
}