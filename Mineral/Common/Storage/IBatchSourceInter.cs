using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public interface IBatchSourceInter<K, V> : ISourceInter<K, V>
    {
        void UpdateByBatch(Dictionary<K, V> rows);
        void UpdateByBatch(Dictionary<K, V> rows, WriteOptionWrapper options);
    }
}
