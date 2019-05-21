using Mineral.Core2;
using Mineral.Core2.Transactions;
using Mineral.Old;
using System.Collections.Generic;
using System.IO;

namespace Mineral.Network.Payload
{
	internal class BlocksPayload : ISerializable
	{
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
			Blocks = reader.ReadSerializableArray<Block>((int)PrevConfig.Instance.Block.PayloadCapacity);
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.WriteSerializableArray(Blocks);
		}
	}

	public class BroadcastBlockPayload : ISerializable
	{
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
			Blocks = reader.ReadSerializableArray<Block>((int)PrevConfig.Instance.Block.PayloadCapacity);
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.WriteSerializableArray(Blocks);
		}
	}

    public class TransactionsPayload : ISerializable
    {
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
            Transactions = reader.ReadSerializableArray<Transaction>((int)PrevConfig.Instance.Transaction.PayloadCapacity);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteSerializableArray(Transactions);
        }
    }
}
