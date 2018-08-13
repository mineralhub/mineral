using System;
using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class DataTransaction : TransactionBase
    {
        public byte[] Data;
        public override int Size => base.Size + Data.GetSize();

        public DataTransaction()
        {
        }

        public DataTransaction(Transaction owner, List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
            : base(owner, inputs, outputs, signatures)
        {
        }

        public override void CalcFee()
        {
            Fee = Fixed8.Satoshi * 1000 * Data.LongLength;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Data = reader.ReadByteArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteByteArray(Data);
        }
    }
}
