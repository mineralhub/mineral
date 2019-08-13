using Mineral.Core.Cache.Entry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Queue
{
    public class WriteQueue<TKey, TValue>
    {
        #region Field
        private IReferenceEntry<TKey, TValue> head = new DefaultHeadEntry<TKey, TValue>();
        #endregion


        #region Property
        public bool IsEmpty
        {
            get { return this.head.NextInWriteQueue == this.head; }
        }

        public int Count
        {
            get
            {
                int size = 0;
                for (IReferenceEntry<TKey, TValue> entry = this.head.NextInWriteQueue; entry != head; entry = entry.NextInWriteQueue)
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
            return entry.NextInWriteQueue != NullEntry<TKey, TValue>.Instance;
        }

        public void Enqueue(IReferenceEntry<TKey, TValue> entry)
        {
            ConnectWriteOrder(entry.PrevInWriteQueue, entry.NextInWriteQueue);
            ConnectWriteOrder(this.head.PrevInWriteQueue, entry);
            ConnectWriteOrder(entry, this.head);
        }

        public IReferenceEntry<TKey, TValue> Dequeue()
        {
            IReferenceEntry<TKey, TValue> next = this.head.NextInWriteQueue;
            if (next.Equals(this.head))
                return null;

            Remove(next);

            return next;
        }

        public IReferenceEntry<TKey, TValue> Peek()
        {
            IReferenceEntry<TKey, TValue> next = this.head.NextInWriteQueue;
            return (next.Equals(head)) ? null : next;
        }

        public bool Remove(IReferenceEntry<TKey, TValue> entry)
        {
            IReferenceEntry<TKey, TValue> prev = entry.PrevInWriteQueue;
            IReferenceEntry<TKey, TValue> next = entry.NextInWriteQueue;
            ConnectWriteOrder(prev, next);
            NullifyWriteOrder(entry);

            return next != NullEntry<TKey, TValue>.Instance;
        }

        public void Clear()
        {
            IReferenceEntry<TKey, TValue> entry = this.head.NextInWriteQueue;

            while (entry != this.head)
            {
                IReferenceEntry<TKey, TValue> next = entry.NextInWriteQueue;
                NullifyWriteOrder(entry);
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

        public static void ConnectWriteOrder(IReferenceEntry<TKey, TValue> prev, IReferenceEntry<TKey, TValue> next)
        {
            prev.NextInWriteQueue = next;
            next.PrevInWriteQueue = prev;
        }

        public static void NullifyWriteOrder(IReferenceEntry<TKey, TValue> nulled)
        {
            IReferenceEntry<TKey, TValue> null_entry = NullEntry<TKey, TValue>.Instance;
            nulled.PrevInWriteQueue = null_entry;
            nulled.NextInWriteQueue = null_entry;
        }
        #endregion
    }
}
