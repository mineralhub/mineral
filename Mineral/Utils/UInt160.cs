using System;

namespace Mineral.Utils
{
    public class UInt160 : UIntBase, IComparable<UInt160>, IEquatable<UInt160>
    {
        public static readonly UInt160 Zero = new UInt160();

        public UInt160(byte[] value)
            : base (20, value)
        {
        }

        public UInt160()
            : this(null)
        {
        }

        public static bool operator >(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static UInt160 FromHexString(string str, bool hasPrefix)
        {
            if (hasPrefix)
                str = str.Substring(2, str.Length - 2);

            return new UInt160(Helper.HexToBytes(str));
        }

        public int CompareTo(UInt160 other)
        {
            return base.CompareTo(other);
        }

        public bool Equals(UInt160 other)
        {
            return base.Equals(other);
        }
    }
}
