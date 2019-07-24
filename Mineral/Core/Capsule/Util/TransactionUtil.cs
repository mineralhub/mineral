using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Protocol;
using static Protocol.Transaction.Types;

namespace Mineral.Core.Capsule.Util
{
    public static class TransactionUtil
    {
        public static Transaction NewGenesisTransaction(byte[] key, long value)
        {

            if (!Wallet.IsValidAddress(key))
            {
                throw new ArgumentException("Invalid address");
            }
            TransferContract contract = new TransferContract();
            contract.Amount = value;
            contract.OwnerAddress = ByteString.CopyFrom(Encoding.UTF8.GetBytes("0x0000000000000000000"));
            contract.ToAddress = ByteString.CopyFrom(key);

            return new TransactionCapsule(contract, Contract.Types.ContractType.TransferContract).Instance;
        }

        public static bool ValidAccountName(byte[] account_name)
        {
            if (account_name == null || account_name.Length <= 0)
                return true;

            return account_name.Length <= 200;
        }

        public static bool ValidAccountId(byte[] account_id)
        {
            if (account_id == null || account_id.Length <= 0)
                return false;

            if (account_id.Length < 8)
                return false;

            if (account_id.Length > 32)
                return false;

            foreach (byte b in account_id)
            {
                if (b < 0x21) // 0x21 = '!'
                {
                    return false;
                }
                if (b > 0x7E) // 0x7E = '~'
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ValidAssetName(byte[] asset_name)
        {
            if (asset_name == null || asset_name.Length <= 0)
                return false;

            if (asset_name.Length > 32)
                return false;

            foreach (byte b in asset_name)
            {
                if (b < 0x21) // 0x21 = '!'
                {
                    return false;
                }
                if (b > 0x7E) // 0x7E = '~'
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ValidTokenAbbrName(byte[] abbr_name)
        {
            if (abbr_name == null || abbr_name.Length <= 0)
                return false;

            if (abbr_name.Length > 5)
                return false;

            foreach (byte b in abbr_name)
            {
                if (b < 0x21) // 0x21 = '!'
                {
                    return false;
                }
                if (b > 0x7E) // 0x7E = '~'
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ValidAssetDescription(byte[] description)
        {
            if (description == null || description.Length <= 0)
            {
                return true;
            }

            return description.Length <= 200;
        }

        public static bool ValidUrl(byte[] url)
        {
            if (url == null || url.Length <= 0)
                return false;

            return url.Length <= 256;
        }

        public static bool IsNumber(byte[] id)
        {
            if (id == null || id.Length <= 0)
                return false;

            foreach (byte b in id)
            {
                if (b < '0' || b > '9')
                {
                    return false;
                }
            }

            return !(id.Length > 1 && id[0] == '0');
        }
    }
}
