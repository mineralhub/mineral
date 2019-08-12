using Mineral.Core.Cache.Entry;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mineral.Core.Cache
{
    public interface IValueReference<TKey, TValue>
    {
        IReferenceEntry<TKey, TValue> Entry { get; }
        int Weight { get; }
        bool IsLoading { get; }
        bool IsActive { get; }

        TValue Get();
        TValue WaitForValue();

        IValueReference<TKey, TValue> Copy(Queue<TValue> queue, TValue value, IReferenceEntry<TKey, TValue> entry);
        void NotifyNewValue(TValue value);
    }
}
