using Google.Protobuf;
using Mineral;
using Mineral.Core.Capsule.Util;
using Mineral.Cryptography;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace MineralCLI.Api
{
    public static class WalletApi
    {
        public static TransferContract CreateTransaferContract(byte[] owner, byte[] to, long amount)
        {
            TransferContract contract = new TransferContract();
            contract.ToAddress = ByteString.CopyFrom(to);
            contract.OwnerAddress = ByteString.CopyFrom(owner);
            contract.Amount = amount;

            return contract;
        }

        public static Transaction SignatureTransaction(ECKey key, Transaction transaction)
        {
            if (transaction.RawData.Timestamp == 0)
            {
                transaction.RawData.Timestamp = Helper.CurrentTimeMillis();
            }

            transaction.RawData.Expiration = TransactionUtil.GetExpirationTime();

            return transaction;
        }
    }
}
