using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public interface IStatsCounter
    {
        void RecordHits(int count);
        void RecordMisses(int count);
        void RecordLoadSuccess(long time);
        void RecordLoadException(long time);
        void RecordEviction();
        CacheStats Snapshot();
    }
}
