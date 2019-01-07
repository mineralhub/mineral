using Mineral.Utils;
using System.IO;

namespace Mineral.Network.Payload
{
    internal class GetBlocksPayload : ISerializable
    {
        public UInt256 HashStart;
        public UInt256 HashStop;

        public int Size => HashStart.Size + HashStop.Size;

        public static GetBlocksPayload Create(UInt256 hashStart, UInt256 hashStop = null)
        {
            return new GetBlocksPayload
            {
                HashStart = hashStart,
                HashStop = hashStop ?? UInt256.Zero
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            HashStart = reader.ReadSerializable<UInt256>();
            HashStop = reader.ReadSerializable<UInt256>();

        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteSerializable(HashStart);
            writer.WriteSerializable(HashStop);
        }
    }

    internal class GetBlocksFromHeightPayload : ISerializable
    {
        public int Start { get; private set; }
        public int End { get; private set; }

        public int Size => sizeof(int) + sizeof(int);

        public static GetBlocksFromHeightPayload Create(int start, int end = 0)
        {
            return new GetBlocksFromHeightPayload
            {
                Start = start,
                End = end
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            Start = reader.ReadInt32();
            End = reader.ReadInt32();

        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Start);
            writer.Write(End);
        }
    }
}
