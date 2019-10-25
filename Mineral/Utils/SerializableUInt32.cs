using System;
using System.IO;

namespace Mineral.Utils
{
    public class SerializableUInt32 : IEquatable<SerializableUInt32>, IComparable<SerializableUInt32>, ISerializable
    {
        uint Value;
        public int Size => sizeof(uint);

        public SerializableUInt32() { }
        public SerializableUInt32(uint v)
        {
            Value = v;
        }

        public void Deserialize(BinaryReader reader)
        {
            Value = reader.ReadUInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public bool Equals(SerializableUInt32 other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is SerializableUInt32)
                return Equals((SerializableUInt32)obj);
            else
                return Value.Equals(obj);
        }

        public int CompareTo(SerializableUInt32 other)
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

        public static bool operator ==(SerializableUInt32 l, SerializableUInt32 r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(SerializableUInt32 l, SerializableUInt32 r)
        {
            return !(l == r);
        }

        public static bool operator <(SerializableUInt32 l, uint r)
        {
            return l.Value < r;
        }

        public static bool operator >(SerializableUInt32 l, uint r)
        {
            return l.Value > r;
        }

        public static bool operator <=(SerializableUInt32 l, uint r)
        {
            return l.Value <= r;
        }

        public static bool operator >=(SerializableUInt32 l, uint r)
        {
            return l.Value >= r;
        }
    }
}
