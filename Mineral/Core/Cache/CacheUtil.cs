using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public static class CacheUtil
    {
        public static long SaturatedAdd(long value1, long value2)
        {
            long naive_sum = value1 + value2;

            if ((value1 ^ value2) < 0 | (value1 ^ naive_sum) >= 0)
            {
                return naive_sum;
            }

            return long.MaxValue + (((long)((ulong)naive_sum >> (sizeof(long) - 1))) ^ 1);
        }

        public static long SaturatedSubtract(long value1, long value2)
        {
            long diff = value1 - value2;
            if ((value1 ^ value2) >= 0 | (value1 ^ diff) >= 0)
            {
                return diff;
            }

            return long.MaxValue + ((long)((ulong)(diff >> (sizeof(long) - 1))) ^ 1);
        }

    }
}
