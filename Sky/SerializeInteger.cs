using System;
using System.IO;

namespace Sky
{
    public class SerializeInteger : IEquatable<SerializeInteger>, IComparable<SerializeInteger>, ISerializable
    {
        int Value;
        public int Size => sizeof(int);

        public SerializeInteger() { }
        public SerializeInteger(int v)
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

        public bool Equals(SerializeInteger other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is SerializeInteger)
                return Equals((SerializeInteger)obj);
            else
                return Value.Equals(obj);
        }

        public int CompareTo(SerializeInteger other)
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

        public static bool operator ==(SerializeInteger l, SerializeInteger r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(SerializeInteger l, SerializeInteger r)
        {
            return !(l == r);
        }

        public static bool operator <(SerializeInteger l, int r)
        {
            return l.Value < r;
        }

        public static bool operator >(SerializeInteger l, int r)
        {
            return l.Value > r;
        }

        public static bool operator <=(SerializeInteger l, int r)
        {
            return l.Value <= r;
        }

        public static bool operator >=(SerializeInteger l, int r)
        {
            return l.Value >= r;
        }
    }
}
