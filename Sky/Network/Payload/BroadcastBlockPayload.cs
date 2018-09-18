using Sky.Core;
using System.Collections.Generic;
using System.IO;

namespace Sky.Network.Payload
{
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

	public class BroadcastBlockPayload : ISerializable
	{
		public const int MaxCount = 2000;
		public List<Block> Blocks;
		public int Size => Blocks.GetSize();
		public static BroadcastBlockPayload Create(List<Block> blocks)
		{
			return new BroadcastBlockPayload
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
