using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Mineral.Network.Payload
{
    internal class AddressInfo : ISerializable
    {
        public IPEndPoint EndPoint;
        public int Version;
        public uint Timestamp;

        public int Size => sizeof(int) + sizeof(uint) + 16 + sizeof(ushort);

        public static AddressInfo Create(IPEndPoint ep, int version, uint timestamp)
        {
            return new AddressInfo
            {
                EndPoint = ep,
                Version = version,
                Timestamp = timestamp
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                IPAddress address = new IPAddress(reader.ReadBytes(16));
                ushort port = reader.ReadBytes(2).Reverse().ToArray().ToUInt16(0);
                EndPoint = new IPEndPoint(address, port);
                Version = reader.ReadInt32();
                Timestamp = reader.ReadUInt32();
            }
            catch (Exception e)
            {
                throw new FormatException(e.Message, e);
            }

        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(EndPoint.Address.GetAddressBytes());
            writer.Write(BitConverter.GetBytes((ushort)EndPoint.Port).Reverse().ToArray());
            writer.Write(Version);
            writer.Write(Timestamp);
        }
    }

    internal class AddrPayload : ISerializable
    {
        public List<AddressInfo> AddressList;

        public int Size => AddressList.GetSize();

        public static AddrPayload Create(List<AddressInfo> addrs)
        {
            return new AddrPayload
            {
                AddressList = addrs
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            AddressList = reader.ReadSerializableArray<AddressInfo>();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteSerializableArray(AddressList);
        }
    }
}
