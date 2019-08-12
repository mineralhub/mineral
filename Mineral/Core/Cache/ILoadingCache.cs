using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public interface ILoadingCache<TKey, TValue> : ICache<TKey, TValue>
    {
        TValue GetUnchecked(TKey key);
        TValue Apply(TKey key);
        void Refresh(TKey key);
    }
}
