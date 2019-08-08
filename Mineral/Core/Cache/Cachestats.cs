using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public class CacheStats
    {
        #region Field
        private readonly long hit_count = 0;
        private readonly long miss_count = 0;
        private readonly long load_success_count = 0;
        private readonly long load_exception_count = 0;
        private readonly long total_load_time = 0;
        private readonly long eviction_count = 0;
        #endregion


        #region Property
        public long HitCount
        {
            get { return this.hit_count; }
        }

        public double HitRate
        {
            get { return (RequestCount == 0) ? 1.0 : this.hit_count / RequestCount; }
        }

        public long MissCount
        {
            get { return this.miss_count; }
        }

        public double MissRate
        {
            get { return RequestCount == 0 ? 0.0 : this.miss_count / RequestCount; }
        }

        public long LoadSuccessCount
        {
            get { return this.LoadSuccessCount; }
        }

        public long LoadExceptionCount
        {
            get { return this.load_exception_count; }
        }

        public double LoadExceptionRate
        {
            get
            {
                long count = CacheUtil.SaturatedAdd(this.load_success_count, this.load_exception_count);
                return count == 0 ? 0.0 : this.load_exception_count / count;
            }
        }

        public long TotalLoadTime
        {
            get { return this.total_load_time; }
        }

        public double AverageLoadPenalty
        {
            get
            {
                long count = CacheUtil.SaturatedAdd(this.load_success_count, this.load_exception_count);
                return count == 0 ? 0.0 : this.total_load_time / count;
            }
        }

        public long EvictionCount
        {
            get { return this.eviction_count; }
        }

        public long RequestCount
        {
            get { return CacheUtil.SaturatedAdd(this.hit_count, this.miss_count); }
        }

        public long LoadCount
        {
            get { return CacheUtil.SaturatedAdd(this.load_success_count, this.load_exception_count); }
        }
        #endregion


        #region Constructor
        public CacheStats(long hit_count,
                          long miss_count,
                          long load_success_count,
                          long load_exception_count,
                          long total_load_time,
                          long eviction_count)
        {
            this.hit_count = hit_count;
            this.miss_count = miss_count;
            this.load_success_count = load_success_count;
            this.load_exception_count = load_exception_count;
            this.total_load_time = total_load_time;
            this.eviction_count = eviction_count;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public CacheStats Plus(CacheStats other)
        {
            return new CacheStats(
                            CacheUtil.SaturatedAdd(HitCount, other.HitCount),
                            CacheUtil.SaturatedAdd(MissCount, other.MissCount),
                            CacheUtil.SaturatedAdd(LoadSuccessCount, other.LoadSuccessCount),
                            CacheUtil.SaturatedAdd(LoadExceptionCount, other.LoadExceptionCount),
                            CacheUtil.SaturatedAdd(TotalLoadTime, other.TotalLoadTime),
                            CacheUtil.SaturatedAdd(EvictionCount, other.EvictionCount)
                );
        }

        public CacheStats Minus(CacheStats other)
        {
            return new CacheStats(
                            Math.Max(0, CacheUtil.SaturatedSubtract(HitCount, other.HitCount)),
                            Math.Max(0, CacheUtil.SaturatedSubtract(MissCount, other.MissCount)),
                            Math.Max(0, CacheUtil.SaturatedSubtract(LoadSuccessCount, other.LoadSuccessCount)),
                            Math.Max(0, CacheUtil.SaturatedSubtract(LoadExceptionCount, other.LoadExceptionCount)),
                            Math.Max(0, CacheUtil.SaturatedSubtract(TotalLoadTime, other.TotalLoadTime)),
                            Math.Max(0, CacheUtil.SaturatedSubtract(EvictionCount, other.EvictionCount))
                );
        }

        public override int GetHashCode()
        {
            return this.hit_count.GetHashCode() +
                   this.miss_count.GetHashCode() +
                   this.load_success_count.GetHashCode() +
                   this.load_exception_count.GetHashCode() +
                   this.total_load_time.GetHashCode() +
                   this.eviction_count.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || obj.GetType().Equals(GetType()))
                return false;

            CacheStats other = obj as CacheStats;

            return HitCount == other.HitCount
                && MissCount == other.MissCount
                && LoadSuccessCount == other.LoadSuccessCount
                && LoadExceptionCount == other.LoadExceptionCount
                && TotalLoadTime == other.TotalLoadTime
                && EvictionCount == other.EvictionCount;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            return builder.Append("HitCount : " + this.hit_count).Append(" ")
                          .Append("NissCount : " + this.miss_count).Append(" ")
                          .Append("LoadSuccessCount : " + this.load_success_count).Append(" ")
                          .Append("LoadExceptionCount : " + this.load_exception_count).Append(" ")
                          .Append("TotalLoadTime : " + this.total_load_time).Append(" ")
                          .Append("EvictionCount : " + this.eviction_count).Append(" ")
                .ToString();
        }
        #endregion
    }
}
