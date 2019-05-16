using Mineral.Core2;
using System.Collections.Generic;
using System.IO;

namespace Mineral.Network.Payload
{
    internal class HeadersPayload : ISerializable
	{
		public const int MaxCount = 2000;
		public List<BlockHeader> Headers;

		public int Size => Headers.GetSize();

		public static HeadersPayload Create(List<BlockHeader> headers)
		{
			return new HeadersPayload
			{
				Headers = headers
			};
		}

		public void Deserialize(BinaryReader reader)
		{
			Headers = reader.ReadSerializableArray<BlockHeader>(MaxCount);
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.WriteSerializableArray(Headers);
		}
	}
}
