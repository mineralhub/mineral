using System;
using System.IO;

namespace Sky.Core
{
    public class DataTransaction : Transaction
    {
        public byte[] Data { get; private set; }
        public override int Size => base.Size + Data.GetSize();

        public DataTransaction()
        {
        }
        public DataTransaction(byte[] data)
        {
            Data = data;
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
