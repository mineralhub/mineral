using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;
using Protocol;
using static Protocol.SmartContract.Types;
using static Protocol.SmartContract.Types.ABI.Types.Entry.Types;

namespace Mineral.Core
{
    public class Wallet
    {
        #region Field
        public static byte ADDRESS_PREFIX_BYTES = DefineParameter.ADD_PRE_FIX_BYTE_MAINNET;

        private readonly ECKey ec_key = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static byte[] GetSelector(byte[] data)
        {
            if (data == null || data.Length < 4)
                return null;

            byte[] result = new byte[4];
            Array.Copy(data, 0, result, 0, 4);

            return result;
        }

        private static bool IsConstant(ABI abi, byte[] selector)
        {
            bool result = false;

            if (selector == null || selector.Length != 4 || abi.Entrys.Count == 0)
                return false;

            for (int i = 0; i < abi.Entrys.Count; i++)
            {
                ABI.Types.Entry entry = abi.Entrys[i];
                if (entry.Type != ABI.Types.Entry.Types.EntryType.Function)
                {
                    continue;
                }

                int input_count = entry.Inputs.Count;

                StringBuilder sb = new StringBuilder();
                sb.Append(entry.Name);
                sb.Append("(");
                for (int k = 0; k < input_count; k++)
                {
                    var param = entry.Inputs[k];
                    sb.Append(param.Type);
                    if (k + 1 < input_count)
                    {
                        sb.Append(",");
                    }
                }
                sb.Append(")");

                byte[] func_selector = new byte[4];
                Array.Copy(Hash.SHA3(sb.ToString().GetBytes()), 0, func_selector, 0, 4);
                if (func_selector.SequenceEqual(selector))
                {
                    result = entry.Constant == true || entry.StateMutability.Equals(StateMutabilityType.View);
                }
            }

            return result;
        }
        #endregion


        #region External Method
        public static bool IsConstant(ABI abi, TriggerSmartContract trigger_contract)
        {
            try
            {
                bool constant = IsConstant(abi, GetSelector(trigger_contract.Data.ToByteArray()));
                if (constant)
                {
                    if (Args.Instance.VM.SupportConstant == false)
                    {
                        throw new ContractValidateException("this node don't support constant");
                    }
                }
                return constant;
            }
            catch (ContractValidateException e)
            {
                throw e;
            }
            catch
            {
                return false;
            }
        }

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

        public static byte[] Decode58Check(string input)
        {
            byte[] data = Base58.Decode(input);
            if (data.Length <= 4)
            {
                return null;
            }
            byte[] decodeData = new byte[data.Length - 4];
            Array.Copy(data, 0, decodeData, 0, decodeData.Length);
            byte[] hash0 = SHA256Hash.ToHash(decodeData);
            byte[] hash1 = SHA256Hash.ToHash(hash0);
            if (hash1[0] == data[decodeData.Length] &&
                hash1[1] == data[decodeData.Length + 1] &&
                hash1[2] == data[decodeData.Length + 2] &&
                hash1[3] == data[decodeData.Length + 3])
            {
                return decodeData;
            }
            return null;
        }

        public static byte[] DecodeFromBase58Check(string address)
        {
            byte[] result = null;

            if (StringHelper.IsNullOrEmpty(address))
            {
                Logger.Warning("Address is empty");
                return result;
            }

            result = Decode58Check(address);
            AddressValid(result);

            return result;
        }

        public static bool AddressValid(byte[] address)
        {
            if (address.IsNullOrEmpty())
            {
                Logger.Warning("Warning: Address is empty !!");
                return false;
            }
            if (address.Length != DefineParameter.ADDRESS_SIZE / 2)
            {
                Logger.Warning(
                    "Warning: Address length need " + DefineParameter.ADDRESS_SIZE + " but " + address.Length + " !!");
                return false;
            }
            if (address[0] != ADDRESS_PREFIX_BYTES)
            {
                Logger.Warning("Warning: Address need prefix with " + ADDRESS_PREFIX_BYTES + " but "
                    + address[0] + " !!");
                return false;
            }

            return true;
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

            return Hash.SHA3omit12(combined);
        }

        public static byte[] GenerateContractAddress(byte[] owner_address, byte[] tx_hash)
        {
            byte[] combined = new byte[tx_hash.Length + owner_address.Length];
            Array.Copy(tx_hash, 0, combined, 0, tx_hash.Length);
            Array.Copy(owner_address, 0, combined, tx_hash.Length, owner_address.Length);

            return Hash.SHA3omit12(combined);
        }

        public static byte[] GenerateContractAddress(byte[] tx_root_id, long nonce)
        {
            byte[] nonce_bytes = BitConverter.GetBytes(nonce);
            byte[] combined = new byte[tx_root_id.Length + nonce_bytes.Length];
            Array.Copy(tx_root_id, 0, combined, 0, tx_root_id.Length);
            Array.Copy(nonce_bytes, 0, combined, tx_root_id.Length, nonce_bytes.Length);

            return Hash.SHA3omit12(combined);
        }

        public static byte[] GenerateContractAddress2(byte[] address, byte[] salt, byte[] code)
        {
            byte[] merge = address.Concat(salt).Concat(Hash.SHA3(code)).ToArray();
            return Hash.SHA3omit12(merge);
        }

        public static byte[] ToMineralAddress(byte[] address)
        {
            byte[] result = address;
            if (address.Length == 20)
            {
                result = new byte[21];
                result[0] = ADDRESS_PREFIX_BYTES;
                Array.Copy(address, 0, result, 1, address.Length);
            }

            return result;
        }
        #endregion
    }
}
