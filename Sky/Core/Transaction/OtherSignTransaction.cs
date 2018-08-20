using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class OtherSignTransaction : TransactionBase
    {
        public HashSet<string> Others;
        public int ValidBlockHeight;

        public OtherSignTransaction(Transaction owner, List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
            : base(owner, inputs, outputs, signatures)
        {
            Others = new HashSet<string>();
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Others = reader.ReadStringHashSet();
            ValidBlockHeight = reader.ReadInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteStringHashSet(Others);
            writer.Write(ValidBlockHeight);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;

            if (Others.Count == 0)
                return false;

            if (ValidBlockHeight < Blockchain.Instance.CurrentBlockHeight)
                return false;
            
            foreach (string addr in Others)
            {
                if (Wallets.WalletAccount.IsAddress(addr))
                    return false;
            }
            return true;
        }
    }
}