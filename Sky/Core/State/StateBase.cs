using System;
using System.IO;

namespace Sky.Core
{
    public abstract class StateBase : ISerializable
    {
        public virtual int Size => sizeof(byte);

        public virtual void Deserialize(BinaryReader reader)
        {
            if (reader.ReadByte() != Config.StateVersion)
                throw new FormatException();
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(Config.StateVersion);
        }
    }
}
