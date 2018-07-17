using System;
using System.IO;
using System.Text;

namespace Sky.Core
{
    public class DelegateKey : IEquatable<DelegateKey>, ISerializable
    {
        public byte[] NameBytes { get; private set; }
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = Encoding.UTF8.GetString(NameBytes);
                return _name;
            }
        }
        private string _name;

        public int Size => NameBytes.Length;

        public DelegateKey() { }
        public DelegateKey(byte[] nameBytes)
        {
            NameBytes = nameBytes;
        }

        public DelegateKey(string name)
        {
            NameBytes = Encoding.UTF8.GetBytes(name);
        }

        public void Deserialize(BinaryReader reader)
        {
            NameBytes = reader.ReadByteArray();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteByteArray(NameBytes);
        }

        public bool Equals(DelegateKey other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return NameBytes.Equals(other.NameBytes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (!(obj is DelegateKey)) return false;
            return Equals((DelegateKey)obj);
        }

        public override int GetHashCode()
        {
            return NameBytes.GetHashCode();
        }
    }
}
