using System;
using System.Globalization;
using System.IO;

namespace Mineral.Utils
{
    public struct Fixed8 : IComparable<Fixed8>, IEquatable<Fixed8>, IFormattable, ISerializable
    {
        private const long D = 100000000;
        internal long _value;

        public static readonly Fixed8 MaxValue = new Fixed8 { _value = long.MaxValue };
        public static readonly Fixed8 MinValue = new Fixed8 { _value = long.MinValue };
        public static readonly Fixed8 One = new Fixed8 { _value = D };
        public static readonly Fixed8 Satoshi = new Fixed8 { _value = 1 };
        public static readonly Fixed8 Zero = default(Fixed8);

        public long Value => _value;

        public int Size => sizeof(long);

        public Fixed8(long data)
        {
            _value = data;
        }

        public int CompareTo(Fixed8 other)
        {
            return _value.CompareTo(other._value);
        }

        public void Deserialize(BinaryReader reader)
        {
            _value = reader.ReadInt64();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(_value);
        }

        public bool Equals(Fixed8 other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fixed8))
                return false;
            return Equals((Fixed8)obj);
        }

        public static Fixed8 FromLongValue(long value)
        {
            Fixed8 retval;
            checked
            {
                retval = new Fixed8(value * D);
            }
            return retval;
        }

        public static Fixed8 FromDecimal(decimal value)
        {
            value *= D;
            if (value < long.MinValue || long.MaxValue < value)
                throw new OverflowException();
            return new Fixed8 { _value = (long)value };
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static Fixed8 Parse(string s)
        {
            return FromDecimal(decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return ((decimal)this).ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(string format)
        {
            return ((decimal)this).ToString(format);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ((decimal)this).ToString(format, formatProvider);
        }

        public static bool TryParse(string s, out Fixed8 result)
        {
            decimal d;
            if (!decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
            {
                result = default(Fixed8);
                return false;
            }
            d *= D;
            if (d < long.MinValue || d > long.MaxValue)
            {
                result = default(Fixed8);
                return false;
            }
            result = new Fixed8
            {
                _value = (long)d
            };
            return true;
        }

        public static explicit operator decimal(Fixed8 value)
        {
            return value._value / (decimal)D;
        }

        public static explicit operator long(Fixed8 value)
        {
            return value._value / D;
        }

        public static bool operator ==(Fixed8 x, Fixed8 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Fixed8 x, Fixed8 y)
        {
            return !x.Equals(y);
        }

        public static bool operator >(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator >=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) >= 0;
        }

        public static bool operator <=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static Fixed8 operator *(Fixed8 x, Fixed8 y)
        {
            const ulong QUO = (1ul << 63) / (D >> 1);
            const ulong REM = (1ul << 63) % (D >> 1);
            int sign = Math.Sign(x._value) * Math.Sign(y._value);
            ulong ux = (ulong)Math.Abs(x._value);
            ulong uy = (ulong)Math.Abs(y._value);
            ulong xh = ux >> 32;
            ulong xl = ux & 0x00000000fffffffful;
            ulong yh = uy >> 32;
            ulong yl = uy & 0x00000000fffffffful;
            ulong rh = xh * yh;
            ulong rm = xh * yl + xl * yh;
            ulong rl = xl * yl;
            ulong rmh = rm >> 32;
            ulong rml = rm << 32;
            rh += rmh;
            rl += rml;
            if (rl < rml)
                ++rh;
            if (rh >= D)
                throw new OverflowException();
            ulong r = rh * QUO + (rh * REM + rl) / D;
            x._value = (long)r * sign;
            return x;
        }

        public static Fixed8 operator *(Fixed8 x, long y)
        {
            x._value = checked(x._value * y);
            return x;
        }

        public static Fixed8 operator /(Fixed8 x, long y)
        {
            x._value = checked(x._value / y);
            return x;
        }

        public static Fixed8 operator +(Fixed8 x, Fixed8 y)
        {
            x._value = checked(x._value + y._value);
            return x;
        }

        public static Fixed8 operator -(Fixed8 x, Fixed8 y)
        {
            x._value = checked(x._value - y._value);
            return x;
        }

        public static Fixed8 operator -(Fixed8 value)
        {
            value._value = -value._value;
            return value;
        }
    }
}
