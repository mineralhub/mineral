using System;
using System.IO;

namespace Mineral.Utils
{
    public class SerializableInt32 : IEquatable<SerializableInt32>, IComparable<SerializableInt32>, ISerializable
    {
        int Value;
        public int Size => sizeof(int);

        public SerializableInt32() { }
        public SerializableInt32(int v)
        {
            Value = v;
        }

        public void Deserialize(BinaryReader reader)
        {
            Value = reader.ReadInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public bool Equals(SerializableInt32 other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is SerializableInt32)
                return Equals((SerializableInt32)obj);
            else
                return Value.Equals(obj);
        }

        public int CompareTo(SerializableInt32 other)
        {
            return Value.CompareTo(other.Value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(SerializableInt32 l, SerializableInt32 r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(SerializableInt32 l, SerializableInt32 r)
        {
            return !(l == r);
        }

        public static bool operator <(SerializableInt32 l, int r)
        {
            return l.Value < r;
        }

        public static bool operator >(SerializableInt32 l, int r)
        {
            return l.Value > r;
        }

        public static bool operator <=(SerializableInt32 l, int r)
        {
            return l.Value <= r;
        }

        public static bool operator >=(SerializableInt32 l, int r)
        {
            return l.Value >= r;
        }
    }
}
