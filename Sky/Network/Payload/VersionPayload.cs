using System;
using System.IO;

namespace Sky.Network.Payload
{
    public class VersionPayload : ISerializable
    {
        public int Version;
        public int Timestamp;
        public ushort Port;
        public uint Nonce;
        public int Height;
        public bool Relay;

        public int Size => sizeof(int) + sizeof(int) + sizeof(ushort) + sizeof(uint) + sizeof(int) + sizeof(bool);

        public static VersionPayload Create(int port)
        {
            return new VersionPayload
            {
                Version = Config.ProtocolVersion,
                Timestamp = DateTime.Now.ToTimestamp(),
                Port = (ushort)port,
                Nonce = Config.Nonce,
                Height = Core.Blockchain.Instance.CurrentBlockHeight,
                Relay = true
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                Version = reader.ReadInt32();
                Timestamp = reader.ReadInt32();
                Port = reader.ReadUInt16();
                Nonce = reader.ReadUInt32();
                Height = reader.ReadInt32();
                Relay = reader.ReadBoolean();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Timestamp);
            writer.Write(Port);
            writer.Write(Nonce);
            writer.Write(Height);
            writer.Write(Relay);
        }
    }
}
