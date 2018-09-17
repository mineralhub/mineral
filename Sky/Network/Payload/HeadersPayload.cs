using Sky.Core;
using System.Collections.Generic;
using System.IO;

namespace Sky.Network.Payload
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

	internal class BlocksPayload : ISerializable
	{
		public const int MaxCount = 2000;
		public List<Block> Blocks;
		public int Size => Blocks.GetSize();
		public static BlocksPayload Create(List<Block> blocks)
		{
			return new BlocksPayload
			{
				Blocks = blocks
			};
		}

		public void Deserialize(BinaryReader reader)
		{
			Blocks = reader.ReadSerializableArray<Block>(MaxCount);
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.WriteSerializableArray(Blocks);
		}
	}
}
