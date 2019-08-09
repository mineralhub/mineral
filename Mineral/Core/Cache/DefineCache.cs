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
        MICROSECONDS,
        MILLISECONDS,
        SECONDS,
        MINUTES,
        HOURS,
        DAYS,
        YEARS
    }
}
