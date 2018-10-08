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

    public class TransactionsPayload : ISerializable
    {
        public const int MaxCount = 2000;
        public List<Transaction> Transactions;
        public int Size => Transactions.GetSize();
        public static TransactionsPayload Create(List<Transaction> transactions)
        {
            return new TransactionsPayload
            {
                Transactions = transactions
            };
        }

        public static TransactionsPayload Create(Transaction tx)
        {
            TransactionsPayload pl = new TransactionsPayload();
            pl.Transactions = new List<Transaction>();
            pl.Transactions.Add(tx);
            return pl;
        }

        public void Deserialize(BinaryReader reader)
        {
            Transactions = reader.ReadSerializableArray<Transaction>(1);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteSerializableArray(Transactions);
        }
    }
}
