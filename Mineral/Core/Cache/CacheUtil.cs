using Mineral.Core.Cache.Entry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public static class CacheUtil
    {
        public static readonly long NANOS = 1;
        public static readonly long MICROS = NANOS * 1000;
        public static readonly long MILLIS = MICROS * 1000;
        public static readonly long SECONDS = MILLIS * 1000;
        public static readonly long MINUTES = SECONDS * 60;
        public static readonly long HOUR = MINUTES * 60;
        public static readonly long DAY = HOUR * 24;

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

        public static long ValidLong(long d, long m, long over)
        {
            if (d > over) return long.MaxValue;
            if (d < -over) return long.MinValue;
            return d * m;
        }

        public static long TimeToNanos(long duration, TimeUnit unit)
        {
            long result = 0;
            switch (unit)
            {
                case TimeUnit.NANOSECONDS:
                    result = duration;
                    break;
                case TimeUnit.MICROSECONDS:
                    result = ValidLong(duration, MICROS / NANOS, long.MaxValue / (MICROS / NANOS));
                    break;
                case TimeUnit.MILLISECONDS:
                    result = ValidLong(duration, MILLIS / NANOS, long.MaxValue / (MILLIS / NANOS));
                    break;
                case TimeUnit.SECONDS:
                    result = ValidLong(duration, SECONDS / NANOS, long.MaxValue / (SECONDS / NANOS));
                    break;
                case TimeUnit.MINUTES:
                    result = ValidLong(duration, MINUTES / NANOS, long.MaxValue / (MINUTES / NANOS));
                    break;
                case TimeUnit.HOURS:
                    result = ValidLong(duration, HOUR / NANOS, long.MaxValue / (HOUR / NANOS));
                    break;
                case TimeUnit.DAYS:
                    result = ValidLong(duration, DAY / NANOS, long.MaxValue / (DAY / NANOS));
                    break;
                default:
                    {
                        throw new ArgumentException("ToNanos invalidate parameter TimeUnit");
                    }
            }

            return result;
        }

        public static T FirstNonNull<T>(T first, T second)
        {
            if (first != null)
                return first;

            if (second != null)
                return second;

            throw new ArgumentNullException("Bad parameters are null");
        }

        public static IReferenceEntry<TKey, TValue> NewEntry<TKey, TValue>(this EntryFactory factory,
                                                                           Segment<TKey, TValue> segment,
                                                                           TKey key,
                                                                           int hash,
                                                                           IReferenceEntry<TKey, TValue> next)
            where TKey : class
        {
            IReferenceEntry<TKey, TValue> result = null;

            switch (factory)
            {
                case EntryFactory.STRONG:
                    result = new StrongEntry<TKey, TValue>(key, hash, next);
                    break;
                case EntryFactory.STRONG_ACCESS:
                    result = new StrongAccessEntry<TKey, TValue>(key, hash, next);
                    break;
                case EntryFactory.STRONG_WRITE:
                    result = new StrongWriteEntry<TKey, TValue>(key, hash, next);
                    break;
                case EntryFactory.STRONG_ACCESS_WRITE:
                    result = new StrongAccessWriteEntry<TKey, TValue>(key, hash, next);
                    break;
                case EntryFactory.WEAK:
                    result = new WeakEntry<TKey, TValue>(segment.KeyReferenceQueue, key, hash, next);
                    break;
                case EntryFactory.WEAK_ACCESS:
                    result = new WeakAccessEntry<TKey, TValue>(segment.KeyReferenceQueue, key, hash, next);
                    break;
                case EntryFactory.WEAK_ACCESS_WRITE:
                    result = new WeakAccessWriteEntry<TKey, TValue>(segment.KeyReferenceQueue, key, hash, next);
                    break;
                default:
                    throw new ArgumentException();
            }

            return result;
        }

        public static IReferenceEntry<TKey, TValue> CopyEntry<TKey, TValue>(this EntryFactory factory,
                                                                            Segment<TKey, TValue> segment,
                                                                            IReferenceEntry<TKey, TValue> original,
                                                                            IReferenceEntry<TKey, TValue> new_value)
            where TKey : class
        {
            return NewEntry<TKey, TValue>(factory, segment, original.Key, original.Hash, new_value);
        }
    }
}
