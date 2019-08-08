using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public interface IWeigher<TKey, TValue>
    {
        int Weigh(TKey key, TValue value);
    }
}
