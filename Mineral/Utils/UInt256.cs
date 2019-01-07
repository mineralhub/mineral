using System;

namespace Mineral.Utils
{
    public class UInt256 : UIntBase, IComparable<UInt256>, IEquatable<UInt256>
    { 
        public static readonly UInt256 Zero = new UInt256();

        public UInt256(byte[] value)
            : base(32, value)
        {
        }

        public UInt256()
            : this(null)
        {
        }

        public static bool operator >(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static UInt256 FromHexString(string str)
        {
            if (str.StartsWith("0x"))
                str = str.Substring(2, str.Length - 2);
            return new UInt256(Helper.HexToBytes(str));
        }

        public int CompareTo(UInt256 other)
        {
            return base.CompareTo(other);
        }

        public bool Equals(UInt256 other)
        {
            return base.Equals(other);
        }
    }
}
