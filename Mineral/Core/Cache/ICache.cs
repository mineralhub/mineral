using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public interface ICache<TKey, TValue>
    {
        CacheStats Stats { get; }

        TValue Get(TKey key);
        Dictionary<TKey, TValue> GetAll();

        void Put(TKey key, TValue value);
        void PutAll(Dictionary<TKey, TValue> items);
        void Invalidate(TKey key);
        void InvalidateAll(IEnumerable<TKey> keys);
        void InvalidateAll();

        void CleanUp();
    }
}
