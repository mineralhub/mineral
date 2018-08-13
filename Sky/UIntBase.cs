using System;
using System.Linq;
using System.IO;

namespace Sky
{
    public abstract class UIntBase : IEquatable<UIntBase>, IComparable<UIntBase>, ISerializable
    {
        private byte[] _data;
        public byte[] Data => _data;
        public int Size => _data.Length;

        protected UIntBase(int bytes, byte[] value)
        {
            if (value == null)
            {
                this._data = new byte[bytes];
                return;
            }
            if (value.Length != bytes)
                throw new ArgumentException();

            this._data = value;
        }

        public bool Equals(UIntBase other)
        {
            if (ReferenceEquals(other, null))
                return false;
            else if (ReferenceEquals(this, other))
                return true;
            else if (this._data.Length != other._data.Length)
                return false;
            else
                return this._data.SequenceEqual(other._data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            else if (!(obj is UIntBase))
                return false;
            else
                return this.Equals((UIntBase)obj);
        }

        public int CompareTo(UIntBase other)
        {
            byte[] x = Data;
            byte[] y = other.Data;
            for (int i = x.Length - 1; i >= 0; i--)
            {
                if (x[i] > y[i])
                    return 1;
                if (x[i] < y[i])
                    return -1;
            }
            return 0;
        }

        public override int GetHashCode()
        {
            return _data.ToInt32(0);
        }

        public override string ToString()
        {
            return _data.ToHexString();
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                if (reader.BaseStream.Length - reader.BaseStream.Position < _data.Length)
                    throw new EndOfStreamException();

                reader.Read(_data, 0, _data.Length);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            try
            {
                writer.Write(_data);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool operator ==(UIntBase l, UIntBase r)
        {
            if (ReferenceEquals(l, r))
                return true;
            else if (ReferenceEquals(l, null) || ReferenceEquals(r, null))
                return false;
            else
                return l.Equals(r);
        }

        public static bool operator !=(UIntBase l, UIntBase r)
        {
            return !(l == r);
        }
    }
}
