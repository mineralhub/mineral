using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mineral.Core;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;
using Protocol;

namespace Mineral.Common.Runtime.VM
{
    public class InternalTransaction
    {
        public enum TransactionType
        {
            TX_PRECOMPILED_TYPE,
            TX_CONTRACT_CREATION_TYPE,
            TX_CONTRACT_CALL_TYPE,
            TX_UNKNOWN_TYPE,
        }

        public enum ExecutorType
        {
            ET_PRE_TYPE,
            ET_NORMAL_TYPE,
            ET_CONSTANT_TYPE,
            ET_UNKNOWN_TYPE
        }

        #region Field
        private Transaction transaction = null;
        private long value = 0;
        private byte[] hash = null;
        private byte[] parent_hash = null;

        private long nonce = 0;
        private byte[] data = null;
        private byte[] transfer_to_address = null;

        private int deep = 0;
        private int index = 0;
        private string note = "";
        private bool is_rejected = false;
        private byte[] send_address = null;
        private byte[] receive_address = null;
        private byte[] proto_encoded = null;

        private Dictionary<string, long> token_info = new Dictionary<string, long>();
        #endregion


        #region Property
        public Transaction Transaction => this.transaction;
        public long Value => this.value;
        public byte[] Hash => GetHash();
        public byte[] ParentHash => this.parent_hash ?? new byte[0];
        public long Nonce => this.nonce;
        public byte[] Data => this.data ?? new byte[0];
        public byte[] TransferToAddress => this.transfer_to_address;
        public int Deep => this.deep;
        public int Index => this.index;
        public string Note => this.note;
        public bool IsReject => this.is_rejected;
        public byte[] SendAddress => this.send_address ?? new byte[0];
        public byte[] ReceiveAddress => this.receive_address ?? new byte[0];
        public Dictionary<string, long> TokenInfo => this.token_info;
        #endregion


        #region Constructor
        public InternalTransaction(Transaction tx, InternalTransaction.TransactionType tx_type)
        {
            this.transaction = tx;
            TransactionCapsule transaction = new TransactionCapsule(tx);
            this.proto_encoded = transaction.Data;
            this.nonce = 0;
            this.deep = -1;

            if (tx_type == TransactionType.TX_CONTRACT_CREATION_TYPE)
            {
                CreateSmartContract contract = ContractCapsule.GetSmartContractFromTransaction(tx);
                if (contract == null)
                {
                    throw new ContractValidateException("Invalid CreateSmartContract protocol");
                }

                this.send_address = contract.OwnerAddress.ToByteArray();
                this.receive_address = new byte[0];
                this.transfer_to_address = Wallet.GenerateContractAddress(tx);
                this.note = "create";
                this.value = contract.NewContract.CallValue;
                this.data = contract.NewContract.Bytecode.ToByteArray();
                this.token_info.Add(contract.TokenId.ToString(), contract.CallTokenValue);
            }
            else if (tx_type == TransactionType.TX_CONTRACT_CALL_TYPE)
            {
                TriggerSmartContract contract = ContractCapsule.GetTriggerContractFromTransaction(tx);
                if (contract == null)
                {
                    throw new ContractValidateException("Invalid TriggerSmartContract protocol");
                }

                this.send_address = contract.OwnerAddress.ToByteArray();
                this.receive_address = contract.ContractAddress.ToByteArray();
                this.transfer_to_address = (byte[])this.receive_address.Clone();
                this.note = "call";
                this.value = contract.CallValue;
                this.data = contract.Data.ToByteArray();
                this.token_info.Add(contract.TokenId.ToString(), contract.CallTokenValue);
            }
            this.hash = transaction.Id.Hash;
        }

        public InternalTransaction(byte[] parent_hash,
                                    int deep, int index,
                                    byte[] send_address, byte[] transfer_to_address,
                                    long value, byte[] data,
                                    string note,
                                    long nonce,
                                    Dictionary<string, long> token_info)
        {
            this.parent_hash = parent_hash;
            this.deep = deep;
            this.index = index;
            this.send_address = send_address;
            this.transfer_to_address = transfer_to_address;
            this.receive_address = note.Equals("create") ? new byte[0] : transfer_to_address.IsNotNullOrEmpty() ? transfer_to_address : new byte[0];
            this.value = value;
            this.data = data.IsNotNullOrEmpty() ? data : new byte[0];
            this.note = note;
            this.nonce = nonce;
            this.hash = GetHash();

            foreach (KeyValuePair<string, long> token in token_info)
            {
                this.token_info.Add(token.Key, token.Value);
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Reject()
        {
            this.is_rejected = true;
        }

        public byte[] GetHash()
        {
            if (this.hash.IsNotNullOrEmpty())
                return (byte[])this.hash.Clone();

            byte[] plain_message = this.GetEncoded();
            byte[] nonce_bytes = BitConverter.GetBytes(this.nonce);
            byte[] hash_bytes = new byte[plain_message.Length + nonce_bytes.Length];

            Array.Copy(plain_message, 0, hash_bytes, 0, plain_message.Length);
            Array.Copy(nonce_bytes, 0, hash_bytes, plain_message.Length, nonce_bytes.Length);

            this.hash = hash_bytes.SHA256();

            return this.hash;
        }

        public byte[] GetEncoded()
        {
            if (this.proto_encoded != null)
                return (byte[])proto_encoded.Clone();

            byte[] parent_hash_bytes = (byte[])this.parent_hash.Clone();
            if (parent_hash == null)
                parent_hash = new byte[0];

            byte[] value_bytes = BitConverter.GetBytes(this.value);
            byte[] raw = new byte[parent_hash_bytes.Length + this.receive_address.Length + this.data.Length + value_bytes.Length];

            int dest_index = 0;
            Array.Copy(parent_hash, 0, raw, 0, parent_hash.Length);

            dest_index += parent_hash_bytes.Length;
            Array.Copy(this.receive_address, 0, raw, dest_index, this.receive_address.Length);

            dest_index += this.receive_address.Length;
            Array.Copy(this.data, 0, raw, dest_index, this.data.Length);

            dest_index += this.data.Length;
            Array.Copy(value_bytes, 0, raw, dest_index, value_bytes.Length);

            return proto_encoded = raw;
        }
        #endregion
    }
}
