using System.Collections.Generic;
using System.IO;

namespace Mineral.Network.Payload
{
    internal class AddrPayload : ISerializable
    {
        public List<NodeInfo> NodeList;

        public int Size => NodeList.GetSize();

        public static AddrPayload Create(List<NodeInfo> infos)
        {
            return new AddrPayload
            {
                NodeList = infos
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            NodeList = reader.ReadSerializableArray<NodeInfo>();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteSerializableArray(NodeList);
        }
    }
}
