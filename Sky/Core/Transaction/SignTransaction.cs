using System.Collections.Generic;
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

        
        public SignTransaction(Transaction owner, List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
            : base(owner, inputs, outputs, signatures)
        {
        }

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
            foreach (MakerSignature sign in Signatures)
            {
                string addr = Wallets.WalletAccount.ToAddress(sign.Pubkey);
                if (osignTx.Others.Contains(addr))
                    return true;
            }
            return false;
        }
    }
}
