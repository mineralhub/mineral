using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Common
{
    public class Parser
    {
        public abstract class IntegerParser : IValueParser
        {
            protected abstract void ParseInteger(CacheBuilderSpec spec, int value);

            public void Parse(CacheBuilderSpec spec, string key, string value)
            {
                if (value != null && value.Length > 0)
                {
                    try
                    {
                        ParseInteger(spec, int.Parse(value));
                    }
                    catch (System.Exception e)
                    {
                        throw new ArgumentException(
                            string.Format("key {0} value set to {1}, must be integer", key, value));
                    }
                }
                else
                {
                    throw new ArgumentException(
                        string.Format("value of key {0} omitted", key));
                }

                throw new NotImplementedException();
            }
        }

        public abstract class LongParser : IValueParser
        {
            protected abstract void ParseLong(CacheBuilderSpec spec, long value);

            public void Parse(CacheBuilderSpec spec, string key, string value)
            {
                if (value != null && value.Length > 0)
                {
                    try
                    {
                        ParseLong(spec, long.Parse(value));
                    }
                    catch (System.Exception e)
                    {
                        throw new ArgumentException(
                            string.Format("key {0} value set to {1}, must be integer", key, value));
                    }
                }
                else
                {
                    throw new ArgumentException(
                        string.Format("value of key {0} omitted", key));
                }

                throw new NotImplementedException();
            }
        }

        public class InitialCapacityParser : IntegerParser
        {
            protected override void ParseInteger(CacheBuilderSpec spec, int value)
            {
                if (spec.InitialCapacity == null)
                {
                    spec.InitialCapacity = value;
                }
                else
                {
                    throw new ArgumentException(
                        string.Format("initial capacity was alread set to : {0}", spec.InitialCapacity));
                }
            }
        }

        public class MaximumSizeParser : LongParser
        {
            protected override void ParseLong(CacheBuilderSpec spec, long value)
            {
                if (spec.MaximumSize != null)
                    throw new ArgumentException(
                        string.Format("maximum size was already set to {0}", spec.MaximumSize));

                if (spec.MaximumWeight != null)
                    throw new ArgumentException(
                        string.Format("maximum weight was already set to {0}", spec.MaximumWeight));

                spec.MaximumSize = value;
            }
        }

        public class MaximumWeightParser : LongParser
        {
            protected override void ParseLong(CacheBuilderSpec spec, long value)
            {
                if (spec.MaximumSize != null)
                    throw new ArgumentException(
                        string.Format("maximum size was already set to {0}", spec.MaximumSize));

                if (spec.MaximumWeight != null)
                    throw new ArgumentException(
                        string.Format("maximum weight was already set to {0}", spec.MaximumWeight));

                spec.MaximumWeight = value;
            }
        }

        public class ConcurrencyLevelParser : IntegerParser
        {
            protected override void ParseInteger(CacheBuilderSpec spec, int value)
            {
                if (spec.ConcurrencyLevel != null)
                    throw new ArgumentException(
                        string.Format("concurrency level was already set to {0}", spec.MaximumWeight));

                spec.ConcurrencyLevel = value;
            }
        }

        public class KeyStrengthParser : IValueParser
        {
            private readonly Strength strength;

            public KeyStrengthParser(Strength strength)
            {
                this.strength = strength;
            }

            public void Parse(CacheBuilderSpec spec, string key, string value)
            {
                if (value != null)
                    throw new ArgumentException(
                        string.Format("key {0} does not take values", key));

                if (spec.KeyStrength != null)
                    throw new ArgumentException(
                        string.Format("{0} was already set to {1}", key, spec.KeyStrength));

                spec.KeyStrength = strength;
            }
        }

        public class ValueStrengthParser : IValueParser
        {
            private readonly Strength strength;

            public ValueStrengthParser(Strength strength)
            {
                this.strength = strength;
            }

            public void Parse(CacheBuilderSpec spec, string key, string value)
            {
                if (value != null)
                    throw new ArgumentException(
                        string.Format("key {0} does not take values", key));

                if (spec.KeyStrength != null)
                    throw new ArgumentException(
                        string.Format("{0} was already set to {1}", key, spec.KeyStrength));

                spec.ValueStrength = strength;
            }
        }

        public class RecordStatsParser : IValueParser
        {
            public void Parse(CacheBuilderSpec spec, string key, string value)
            {
                if (value != null)
                    throw new ArgumentException("record stats does not take values");

                if (spec.RecordStats != null)
                    throw new ArgumentException("recordStats already set");

                spec.RecordStats = true;
            }
        }

        public abstract class DurationParser : IValueParser
        {
            protected abstract void ParseDuration(CacheBuilderSpec spec, long duration, TimeUnit unit);

            public void Parse(CacheBuilderSpec spec, string key, string value)
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentException(
                        string.Format("value of key {0} omitted", key));

                try
                {
                    char lastChar = value[value.Length - 1];
                    TimeUnit time_unit;
                    switch (lastChar)
                    {
                        case 'd':
                            time_unit = TimeUnit.DAYS;
                            break;
                        case 'h':
                            time_unit = TimeUnit.HOURS;
                            break;
                        case 'm':
                            time_unit = TimeUnit.MINUTES;
                            break;
                        case 's':
                            time_unit = TimeUnit.SECONDS;
                            break;
                        default:
                            throw new ArgumentException(
                                string.Format("key {0} invalid format. was {1}, must end with one of [dDhHmMsS]", key, value));
                    }

                    long duration = long.Parse(value.Substring(0, value.Length - 1));
                    ParseDuration(spec, duration, time_unit);
                }
                catch (System.Exception e)
                {
                    throw new ArgumentException(
                        string.Format("key {0} value set to {1}, must be integer", key, value));
                }
            }
        }

        public class AccessDurationParser : DurationParser
        {
            protected override void ParseDuration(CacheBuilderSpec spec, long duration, TimeUnit unit)
            {
                if (spec.AccessExpirationTimeUnit != null)
                    throw new ArgumentException("ExpireAfterAccess already set");

                spec.AccessExpirationDuration = duration;
                spec.AccessExpirationTimeUnit = unit;
            }
        }

        public class WriteDurationParser : DurationParser
        {
            protected override void ParseDuration(CacheBuilderSpec spec, long duration, TimeUnit unit)
            {
                if (spec.WriteExpirationTimeUnit != null)
                    throw new ArgumentException("ExpireAfterWrite already set");

                spec.WriteExpirationDuration = duration;
                spec.WriteExpirationTimeUnit = unit;
            }
        }

        public class RefreshDurationParser : DurationParser
        {
            protected override void ParseDuration(CacheBuilderSpec spec, long duration, TimeUnit unit)
            {
                if (spec.RefreshTimeUnit != null)
                    throw new ArgumentException("refreshAfterWrite already set");

                spec.RefreshDuration = duration;
                spec.RefreshTimeUnit = unit;
            }
        }
    }
}
