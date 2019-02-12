using System.IO;
using Newtonsoft.Json.Linq;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System.Collections.Generic;
using Mineral.Core.State;

namespace Mineral.Core.Transactions
{
    public class SignTransaction : TransactionBase
    {
        public List<UInt256> TxHashes;

        private List<OtherSignTransaction> _reference;
        public List<OtherSignTransaction> Reference 
        {
            get 
            {
                if (_reference == null)
                {
                    _reference = new List<OtherSignTransaction>();
                    foreach (var hash in TxHashes)
                    {
                        TransactionState txState = BlockChain.Instance.GetTransaction(hash);
                        if (txState != null)
                            _reference.Add(txState.Transaction.Data as OtherSignTransaction);
                    }
                }
                return _reference;
            }
        }

        public override int Size => base.Size + TxHashes.GetSize();

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TxHashes = reader.ReadSerializableArray<UInt256>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableArray(TxHashes);
        }

        public override bool Verify()
        {
            return base.Verify();
        }

        public override bool VerifyBlockChain(Storage storage)
        {
            if (!base.VerifyBlockChain(storage))
                return false;

            foreach (var hash in TxHashes)
            {
                TransactionState txState = BlockChain.Instance.GetTransaction(hash);
                if (txState == null || txState.Transaction.Type != TransactionType.OtherSign)
                    return false;

                if (!(txState.Transaction.Data is OtherSignTransaction data) || !data.Others.Contains(Wallets.WalletAccount.ToAddress(Owner.Signature.Pubkey)))
                    return false;
            }
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            JArray hashes = new JArray();
            foreach (var hash in TxHashes)
                hashes.Add(JToken.FromObject(hash.ToString()));
            json["hashes"] = hashes;

            return json;
        }
    }
}
