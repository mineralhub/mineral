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
}
