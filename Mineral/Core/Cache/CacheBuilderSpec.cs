using Mineral.Core.Cache.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public class CacheBuilderSpec
    {
        #region Field
        private static readonly char KEY_SPLIT = ',';
        private static readonly char KEY_VALUE_SPLIT = '=';

        private static Dictionary<string, IValueParser> VALUE_PARSERS = new Dictionary<string, IValueParser>()
        {
            { "initialCapacity", new Parser.InitialCapacityParser() },
            { "maximumSize", new Parser.MaximumSizeParser() },
            { "maximumWeight", new Parser.MaximumWeightParser() },
            { "concurrencyLevel", new Parser.ConcurrencyLevelParser() },
            { "weakKeys", new Parser.KeyStrengthParser(Strength.WEAK) },
            { "softValues", new Parser.ValueStrengthParser(Strength.SOFT) },
            { "weakValues", new Parser.ValueStrengthParser(Strength.WEAK) },
            { "recordStats", new Parser.RecordStatsParser() },
            { "expireAfterAccess", new Parser.AccessDurationParser() },
            { "expireAfterWrite", new Parser.WriteDurationParser() },
            { "refreshAfterWrite", new Parser.RefreshDurationParser() },
            { "refreshInterval", new Parser.RefreshDurationParser() }
        };

        private readonly string specification = "";
        #endregion


        #region Property
        public int? InitialCapacity { get; set; }
        public long? MaximumSize { get; set; }
        public long? MaximumWeight { get; set; }
        public int? ConcurrencyLevel { get; set; }
        public Strength? KeyStrength { get; set; }
        public Strength? ValueStrength { get; set; }
        public bool? RecordStats { get; set; }
        public long WriteExpirationDuration { get; set; }
        public TimeUnit? WriteExpirationTimeUnit { get; set; }
        public long AccessExpirationDuration { get; set; }
        public TimeUnit? AccessExpirationTimeUnit { get; set; }
        public long RefreshDuration { get; set; }
        public TimeUnit? RefreshTimeUnit { get; set; }
        #endregion


        #region Constructor
        public CacheBuilderSpec(string specification)
        {
            this.specification = specification;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static long? DurationInNanos(long duration, TimeUnit? unit)
        {
            if (unit == null)
                return null;

            return CacheUtil.TimeToNanos(duration, (TimeUnit)unit);
        }
        #endregion


        #region External Method
        public static CacheBuilderSpec Parse(string specification)
        {
            CacheBuilderSpec spec = new CacheBuilderSpec(specification);
            if (specification != null && specification.Length > 0)
            {
                foreach (string split in specification.Split(KEY_SPLIT))
                {
                    string[] pair = split.Split(KEY_VALUE_SPLIT);

                    if (pair == null || pair.Length <= 2)
                    {
                        throw new ArgumentException("invalidate key value");
                    }

                    string key = pair[0];
                    if (VALUE_PARSERS.TryGetValue(key, out IValueParser value_parser))
                    {
                        value_parser.Parse(spec, key, pair.Length == 1 ? null : pair[1]);
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("unknown key {0}", key));
                    }
                }
            }

            return spec;
        }

        public static CacheBuilderSpec DisableCaching()
        {
            // Maximum size of zero is one way to block caching
            return CacheBuilderSpec.Parse("maximumSize=0");
        }

        public CacheBuilder<object, object> ToCacheBuilder()
        {
            CacheBuilder<object, object> builder = CacheBuilder<object, object>.NewBuilder();

            if (InitialCapacity != null)
            {
                builder.InitialCapacity = (int)InitialCapacity;
            }
            if (MaximumSize != null)
            {
                builder.MaximumSize = (long)MaximumSize;
            }
            if (MaximumWeight != null)
            {
                builder.MaximumWeight = (long)MaximumWeight;
            }
            if (ConcurrencyLevel != null)
            {
                builder.ConcurrencyLevel = (int)ConcurrencyLevel;
            }
            if (KeyStrength != null)
            {
                switch (KeyStrength)
                {
                    case Strength.WEAK:
                        builder.WeakKeys();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            if (ValueStrength != null)
            {
                switch (ValueStrength)
                {
                    case Strength.SOFT:
                        builder.SoftValues();
                        break;
                    case Strength.WEAK:
                        builder.WeakValues();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            if (RecordStats != null && RecordStats == true)
            {
                builder.RecordStats();
            }
            if (WriteExpirationTimeUnit != null)
            {
                builder.ExpireAfterWrite(WriteExpirationDuration, (TimeUnit)WriteExpirationTimeUnit);
            }
            if (AccessExpirationTimeUnit != null)
            {
                builder.ExpireAfterAccess(AccessExpirationDuration, (TimeUnit)AccessExpirationTimeUnit);
            }
            if (RefreshTimeUnit != null)
            {
                builder.RefreshAfterWrite(RefreshDuration, (TimeUnit)RefreshTimeUnit);
            }

            return builder;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() +
                   InitialCapacity.GetHashCode() +
                   MaximumSize.GetHashCode() +
                   MaximumWeight.GetHashCode() +
                   ConcurrencyLevel.GetHashCode() +
                   KeyStrength.GetHashCode() +
                   ValueStrength.GetHashCode() +
                   RecordStats.GetHashCode() +
                   DurationInNanos(WriteExpirationDuration, WriteExpirationTimeUnit).GetHashCode() +
                   DurationInNanos(AccessExpirationDuration, AccessExpirationTimeUnit).GetHashCode() +
                   DurationInNanos(RefreshDuration, RefreshTimeUnit).GetHashCode();

        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (!(obj is CacheBuilderSpec))
                return false;

            CacheBuilderSpec other = (CacheBuilderSpec)obj;
            return object.Equals(InitialCapacity, other.InitialCapacity)
                && object.Equals(MaximumSize, other.MaximumSize)
                && object.Equals(MaximumWeight, other.MaximumWeight)
                && object.Equals(ConcurrencyLevel, other.ConcurrencyLevel)
                && object.Equals(KeyStrength, other.KeyStrength)
                && object.Equals(ValueStrength, other.ValueStrength)
                && object.Equals(RecordStats, other.RecordStats)
                && object.Equals(
                    DurationInNanos(WriteExpirationDuration, WriteExpirationTimeUnit),
                    DurationInNanos(other.WriteExpirationDuration, other.WriteExpirationTimeUnit))
                && object.Equals(
                    DurationInNanos(AccessExpirationDuration, AccessExpirationTimeUnit),
                    DurationInNanos(other.AccessExpirationDuration, other.AccessExpirationTimeUnit))
                && object.Equals(
                    DurationInNanos(RefreshDuration, RefreshTimeUnit),
                    DurationInNanos(other.RefreshDuration, other.RefreshTimeUnit));
        }

        public string ToParsableString()
        {
            return this.specification;
        }

        public override string ToString()
        {
            return base.ToString() + ToParsableString();
        }
        #endregion
    }
}
