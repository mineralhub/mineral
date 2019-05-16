using System;
using System.IO;

namespace Mineral.Core2
{
    public abstract class StateBase : ISerializable
    {
        public virtual int Size => sizeof(byte);

        public virtual void Deserialize(BinaryReader reader)
        {
            if (reader.ReadByte() != Config.Instance.StateVersion)
                throw new FormatException();
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(Config.Instance.StateVersion);
        }
    }
}
