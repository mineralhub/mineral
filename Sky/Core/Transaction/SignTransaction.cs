using System.IO;

namespace Sky.Core
{
    public class SignTransaction : TransactionBase
    {
        public UInt256 SignTxHash;

        private OtherSignTransaction _reference;
        public OtherSignTransaction Reference 
        {
            get 
            {
                if (_reference == null)
                    _reference = Blockchain.Instance.GetTransaction(SignTxHash).Data as OtherSignTransaction;

                return _reference;
            }
        }

        public override int Size => base.Size + SignTxHash.Size;

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            SignTxHash = reader.ReadSerializable<UInt256>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(SignTxHash);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;

            Transaction tx = Blockchain.Instance.GetTransaction(SignTxHash);
            if (tx == null || tx.Type != eTransactionType.OtherSignTransaction)
                return false;

            OtherSignTransaction osignTx = tx.Data as OtherSignTransaction;
            if (osignTx.Others.Contains(Wallets.WalletAccount.ToAddress(Owner.Signature.Pubkey)))
                return true;
            return false;
        }
    }
}
