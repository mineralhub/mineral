using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public enum Strength
    {
        STRONG,
        SOFT,
        WEAK
    }

    public enum RemovalCause
    {
        EXPLICIT,
        REPLACED,
        COLLECTED,
        EXPIRED,
        SIZE
    }

    public enum TimeUnit
    {
        NANOSECONDS,
        MICROSECONDS,
        MILLISECONDS,
        SECONDS,
        MINUTES,
        HOURS,
        DAYS,
    }

    public enum EntryFactory
    {
        STRONG,
        STRONG_ACCESS,
        STRONG_WRITE,
        STRONG_ACCESS_WRITE,
        WEAK,
        WEAK_ACCESS,
        WEAK_ACCESS_WRITE
    }
}
