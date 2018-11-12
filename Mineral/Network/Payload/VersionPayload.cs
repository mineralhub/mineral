using System;
using System.IO;

namespace Mineral.Network.Payload
{
    public class VersionPayload : ISerializable
    {
        public int Version;
        public int Timestamp;
        public ushort Port;
        public uint Nonce;
        public int Height;
        public bool Relay;
        public Guid NodeID;

        public int Size => sizeof(int) + sizeof(int) + sizeof(ushort) + sizeof(uint) + sizeof(int) + sizeof(bool) + 16/*Guid bytes*/;

        public static VersionPayload Create(int port, Guid _guid)
        {
            return new VersionPayload
            {
                Version = Config.Instance.ProtocolVersion,
                Timestamp = DateTime.Now.ToTimestamp(),
                Port = (ushort)port,
                Nonce = Config.Instance.Nonce,
                Height = Core.Blockchain.Instance.CurrentBlockHeight,
                Relay = true,
                NodeID = _guid
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
                NodeID = new Guid(reader.ReadBytes(16));
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
            writer.Write(NodeID.ToByteArray());
        }
    }

    public class PingPayload : ISerializable
    {
        public int Timestamp;
        public int Height;

        public int Size => sizeof(int) + sizeof(int);

        public static VersionPayload Create()
        {
            return new VersionPayload
            {
                Timestamp = DateTime.Now.ToTimestamp(),
                Height = Core.Blockchain.Instance.CurrentBlockHeight,
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                Timestamp = reader.ReadInt32();
                Height = reader.ReadInt32();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Timestamp);
            writer.Write(Height);
        }
    }
}
