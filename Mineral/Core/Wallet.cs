using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Protocol;

namespace Mineral.Core
{
    public class Wallet
    {
        #region Field
        private readonly ECKey ec_key = null;
        private MineralNetService net_service;
        private NetDelegate net_delegate;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static bool CheckPermissionOperations(Permission permission, Transaction.Types.Contract contract)
        {
            ByteString operations = permission.Operations;
            if (operations.Length != 32)
            {
                throw new PermissionException("Operations size must 32 bytes");
            }
            int contract_type = (int)contract.Type;

            return (operations[(int)(contract_type / 8)] & (1 << (contract_type % 8))) != 0;
        }

        public static string Encode58Check(byte[] input)
        {
            byte[] hash0 = SHA256Hash.ToHash(input);
            byte[] hash1 = SHA256Hash.ToHash(input);
            byte[] input_check = new byte[input.Length + 4];
            Array.Copy(input, 0, input_check, 0, input.Length);
            Array.Copy(hash1, 0, input_check, input.Length, 4);

            return Base58.Encode(input_check);
        }

        public static byte[] GenerateContractAddress(Transaction tx)
        {
            CreateSmartContract contract = ContractCapsule.GetSmartContractFromTransaction(tx);
            byte[] owner_address = contract.OwnerAddress.ToByteArray();
            TransactionCapsule transaction = new TransactionCapsule(tx);
            byte[] tx_hash = transaction.Id.Hash;

            byte[] combined = new byte[tx_hash.Length + owner_address.Length];
            Array.Copy(tx_hash, 0, combined, 0, tx_hash.Length);
            Array.Copy(owner_address, 0, combined, tx_hash.Length, owner_address.Length);

            return Wallets.WalletAccount.ToAddressHash(combined.SHA256()).ToArray();
        }

        public static byte[] GenerateContractAddress(byte[] owner_address, byte[] tx_hash)
        {
            byte[] combined = new byte[tx_hash.Length + owner_address.Length];
            Array.Copy(tx_hash, 0, combined, 0, tx_hash.Length);
            Array.Copy(owner_address, 0, combined, tx_hash.Length, owner_address.Length);

            return Wallets.WalletAccount.ToAddressHash(combined.SHA256()).ToArray();
        }
        #endregion
    }
}
