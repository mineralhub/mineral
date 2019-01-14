using Mineral.Utils;
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

        public static VersionPayload Create(int port, Guid guid)
        {
            return new VersionPayload
            {
                Version = Config.Instance.ProtocolVersion,
                Timestamp = DateTime.Now.ToTimestamp(),
                Port = (ushort)port,
                Nonce = Config.Instance.Nonce,
                Height = Core.BlockChain.Instance.CurrentBlockHeight,
                Relay = true,
                NodeID = guid
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

        public override int GetHashCode()
        {
            return NodeID.GetHashCode();
        }
    }

    public class PingPayload : ISerializable
    {
        public long Timestamp;
        public int Size => sizeof(long);

        public static PingPayload Create()
        {
            return new PingPayload
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                Timestamp = reader.ReadInt64();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Timestamp);
        }
    }

    public class PongPayload : ISerializable
    {
        public long Ping;
        public long Pong;
        public int Height;

        public int Size => sizeof(long) + sizeof(long) + sizeof(int);
        public long LatencyMs => Pong - Ping;

        public static PongPayload Create(long pingtime, int height)
        {
            return new PongPayload
            {
                Ping = pingtime,
                Pong = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Height = height
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                Ping = reader.ReadInt64();
                Pong = reader.ReadInt64();
                Height = reader.ReadInt32();
            }
            catch (Exception e)
            {
                Logger.Error("deserialize PongPayload Exception.");
                throw e;
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Ping);
            writer.Write(Pong);
            writer.Write(Height);
        }
    }

    public class VerackPayload : ISerializable
    {
        public Guid NodeID;

        public int Size => 16/*Guid bytes*/;

        public static VerackPayload Create(Guid _guid)
        {
            return new VerackPayload
            {
                NodeID = _guid
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                NodeID = new Guid(reader.ReadBytes(16));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(NodeID.ToByteArray());
        }
    }
}
