using System.Collections.Generic;
using System.IO;
using Sky.Wallets;

namespace Sky.Core
{
    public class OtherSignTransactionState : StateBase
    {
        public UInt256 TxHash { get; private set; }
        public HashSet<string> RemainSign { get; private set; }

        public override int Size => base.Size + TxHash.Size + RemainSign.GetSize();

        public OtherSignTransactionState()
        {
            RemainSign = new HashSet<string>();
        }

        public OtherSignTransactionState(UInt256 txhash, HashSet<string> remainSign)
        {
            TxHash = txhash;
            RemainSign = remainSign;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TxHash = reader.ReadSerializable<UInt256>();
            RemainSign = reader.ReadStringHashSet();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(TxHash);
            writer.WriteStringHashSet(RemainSign);
        }

        public void Sign(List<MakerSignature> signatures)
        {
            foreach (MakerSignature sign in signatures)
                RemainSign.Remove(WalletAccount.ToAddress(sign.Pubkey));
        }
    }
}
