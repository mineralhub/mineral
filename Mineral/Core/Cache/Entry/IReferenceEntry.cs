using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public interface IReferenceEntry<TKey, TValue>
    {
        TKey Key { get; }
        int Hash { get; }
        IReferenceEntry<TKey, TValue> Next { get; }
        long AccessTime { get; set; }
        long WriteTime { get; set; }

        IValueReference<TKey, TValue> ValueReference { get; set; }
        IReferenceEntry<TKey, TValue> PrevInAccessQueue { get; set; }
        IReferenceEntry<TKey, TValue> PrevInWriteQueue { get; set; }
        IReferenceEntry<TKey, TValue> NextInAccessQueue { get; set; }
        IReferenceEntry<TKey, TValue> NextInWriteQueue { get; set; }
    }
}
