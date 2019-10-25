using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core.Actuator;
using Mineral.Core.Capsule;

namespace Mineral.Common.Utils
{
    public static class MUtil
    {
        public static void Transfer(IDeposit deposit, byte[] from_address, byte[] to_address, long amount)
        {
            if (amount == 0)
                return;

            TransferActuator.ValidateForSmartContract(deposit, from_address, to_address, amount);
            deposit.AddBalance(to_address, amount);
            deposit.AddBalance(from_address, -amount);
        }

        public static void TransferAllToken(IDeposit deposit, byte[] from_address, byte[] to_address)
        {
            AccountCapsule from_account = deposit.GetAccount(from_address);
            AccountCapsule to_account = deposit.GetAccount(to_address);

            foreach (var asset in from_account.AssetV2)
            {
                to_account.AssetV2.TryGetValue(asset.Key, out long value);
                to_account.AddAssetV2(asset.Key, value + asset.Value);
                from_account.AddAssetV2(asset.Key, 0);
            }

            deposit.PutAccountValue(from_address, from_account);
            deposit.PutAccountValue(to_address, to_account);
        }

        public static void TransferToken(IDeposit deposit, byte[] from_address, byte[] to_address, string token_id, long amount)
        {
            if (0 == amount)
                return;

            byte[] token = Encoding.UTF8.GetBytes(token_id);
            TransferAssetActuator.ValidateForSmartContract(deposit, from_address, to_address, token, amount);
            deposit.AddTokenBalance(to_address, token, amount);
            deposit.AddTokenBalance(from_address, token, -amount);
        }
    }
}
