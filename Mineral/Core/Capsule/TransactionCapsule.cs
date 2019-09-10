using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Utils;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;
using Protocol;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Capsule
{
    public class TransactionCapsule : IProtoCapsule<Transaction>
    {
        #region Field
        private Transaction transaction = null;
        private bool is_verifyed = false;
        private long block_num = -1;
        private TransactionTrace transaction_trace = null;
        #endregion


        #region Property
        public Transaction Instance { get { return this.transaction; } }
        public byte[] Data { get { return this.transaction.ToByteArray(); } }

        public long Size
        {
            get { return this.transaction.CalculateSize(); }
        }

        public long ResultSize
        {
            get
            {
                long size = 0;
                foreach (Transaction.Types.Result result in this.transaction.Ret)
                {
                    size += result.CalculateSize();
                }

                return size;
            }
        }

        public SHA256Hash Id
        {
            get { return this.GetRawHash(); }
        }

        public long BlockNum
        {
            get { return this.block_num; }
            set { this.block_num = value; }
        }

        public long Expiration
        {
            get { return this.transaction.RawData.Expiration; }
            set { this.transaction.RawData.Expiration = value; }
        }

        public long Timestamp
        {
            get { return this.transaction.RawData.Timestamp; }
            set { this.transaction.RawData.Timestamp = value; }
        }

        public SHA256Hash MerkleHash
        {
            get { return SHA256Hash.Of(this.transaction.ToByteArray()); }
        }

        public contractResult ContractResult
        {
            get { return this.transaction.Ret.Count > 0 ? this.transaction.Ret[0].ContractRet : contractResult.Unknown; }
        }

        public TransactionTrace TransactionTrace
        {
            get { return this.transaction_trace; }
            set { this.transaction_trace = value; }
        }

        public bool IsVerified
        {
            get { return this.is_verifyed; }
            set { this.is_verifyed = value; }
        }
        #endregion


        #region Constructor
        public TransactionCapsule(Transaction tx)
        {
            this.transaction = tx;
        }

        public TransactionCapsule(byte[] data)
        {
            try
            {
                this.transaction = Transaction.Parser.ParseFrom(data);
            }
            catch
            {
                throw new BadItemException("Transaction proto data parse excepton");
            }
        }

        public TransactionCapsule(CodedInputStream stream)
        {
            try
            {
                this.transaction = Transaction.Parser.ParseFrom(stream);
            }
            catch
            {
                throw new BadItemException("Transaction proto data parse excepton");
            }
        }

        public TransactionCapsule(AccountCreateContract contract, AccountStore account_store)
        {
            AccountCapsule account = account_store.Get(contract.OwnerAddress.ToByteArray());
            if (account != null && account.Type == contract.Type)
                return;

            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.AccountCreateContract);
        }

        public TransactionCapsule(TransferContract contract, AccountStore account_store)
        {
            AccountCapsule account = account_store.Get(contract.OwnerAddress.ToByteArray());
            if (account == null || account.Balance < contract.Amount)
                return;

            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.TransferContract);
        }

        public TransactionCapsule(VoteWitnessContract contract)
        {
            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.VoteWitnessContract);
        }

        public TransactionCapsule(WitnessCreateContract contract)
        {
            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.WitnessCreateContract);
        }

        public TransactionCapsule(WitnessUpdateContract contract)
        {
            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.WitnessUpdateContract);
        }

        public TransactionCapsule(TransferAssetContract contract)
        {
            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.TransferAssetContract);
        }

        public TransactionCapsule(ParticipateAssetIssueContract contract)
        {
            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.WitnessUpdateContract);
        }

        public TransactionCapsule(AssetIssueContract contract)
        {
            CreateTransaction(contract, Transaction.Types.Contract.Types.ContractType.AssetIssueContract);
        }

        public TransactionCapsule(IMessage message, Transaction.Types.Contract.Types.ContractType type)
        {
            CreateTransaction(message, type);
        }

        public TransactionCapsule(Transaction.Types.raw raw, List<ByteString> signatures)
        {
            this.transaction.RawData = raw;
            this.transaction.Signature.AddRange(signatures);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private SHA256Hash GetRawHash()
        {
            return SHA256Hash.Of(this.transaction.RawData.ToByteArray());
        }
        #endregion


        #region External Method
        public static Transaction GenerateGenesisTransaction(byte[] key, long value)
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

        public void CreateTransaction(IMessage message, Transaction.Types.Contract.Types.ContractType contract_type)
        {
            this.transaction = new Transaction() { RawData = new raw() };
            this.transaction.RawData.Contract.Add(new Transaction.Types.Contract()
            {
                Type = contract_type,
                Parameter = Any.Pack(message)
            });
        }

        public void SetTransactionResult(TransactionResultCapsule tx_result)
        {
            this.transaction.Ret.Add(tx_result.Instance);
        }

        public void ClearTransactionResult()
        {
            this.transaction.Ret.Clear();
        }

        public void SetReference(long block_num, byte[] block_hash)
        {
            byte[] ref_block_num = BitConverter.GetBytes(block_num);
            this.transaction.RawData.RefBlockHash = ByteString.CopyFrom(ArrayUtil.SubArray(block_hash, 8, 16));
            this.transaction.RawData.RefBlockBytes = ByteString.CopyFrom(ArrayUtil.SubArray(ref_block_num, 0, 2));
        }

        public void Signature(byte[] privatekey)
        {
            ECKey ec_key = ECKey.FromPrivateKey(privatekey);
            ECDSASignature signature = ec_key.Sign(GetRawHash().Hash);
            this.transaction.Signature.Add(ByteString.CopyFrom(signature.ToByteArray()));
        }

        public static long GetWeight(Permission permission, byte[] address)
        {
            foreach (Key key in permission.Keys)
            {
                if (key.Address.Equals(ByteString.CopyFrom(address)))
                {
                    return key.Weight;
                }
            }

            return 0;
        }

        public static long CheckWeight(Permission permission, List<ByteString> signature, byte[] hash, List<ByteString> approve_list)
        {
            long result = 0;

            if (signature.Count > permission.Keys.Count)
            {
                throw new PermissionException(
                        "Signature count is" + signature.Count +
                        "more than key counts of permission" + permission.Keys.Count);
            }

            Dictionary<ByteString, long> signature_weight = new Dictionary<ByteString, long>();
            foreach (ByteString sign in signature)
            {
                if (sign.Length < 65)
                {
                    throw new SignatureFormatException("Signature size is" + sign.Length);
                }

                ECKey ec_key = ECKey.RecoverFromSignature(ECDSASignature.ExtractECDSASignature(sign.ToByteArray()),
                                                          hash,
                                                          false);

                byte[] publickey = ec_key.PublicKey;
                byte[] address = Wallet.PublickKeyToAddress(ec_key.PublicKey);

                long weight = GetWeight(permission, address);
                if (weight == 0)
                {
                    throw new PermissionException(
                        sign.ToByteArray().ToHexString()
                        + "is signed by"
                        + Wallet.AddressToBase58(address)
                        + "but it is not contained of permission.");
                }

                if (signature_weight.ContainsKey(sign))
                {
                    throw new PermissionException(Wallet.AddressToBase58(address) + " has signed twice");
                }

                signature_weight.Add(sign, weight);
                if (approve_list != null)
                {
                    approve_list.Add(ByteString.CopyFrom(publickey));
                }
                result += weight;
            }

            return result;
        }

        public void AddSignature(byte[] privatekey, AccountStore account_store)
        {
            Transaction.Types.Contract contract = this.transaction.RawData.Contract[0];

            byte[] owner = GetOwner(contract);
            int permission_id = contract.PermissionId;

            AccountCapsule account = account_store.Get(owner);
            if (account == null)
            {
                throw new PermissionException("Account is not exist.");
            }

            Permission permission = account.GetPermissionById(permission_id);
            if (permission == null)
            {
                throw new PermissionException("Permission is not exist");
            }

            if (permission_id != 0)
            {
                if (permission.Type != Permission.Types.PermissionType.Active)
                    throw new PermissionException("Permission type is error");
                if (Wallet.CheckPermissionOperations(permission, contract))
                {
                    throw new PermissionException("Invalid permission");
                }
            }

            List<ByteString> approves = new List<ByteString>();
            ECKey ec_key = ECKey.FromPrivateKey(privatekey);
            byte[] address = Wallet.PublickKeyToAddress(ec_key.PublicKey);

            if (this.transaction.Signature.Count > 0)
            {
                CheckWeight(permission, new List<ByteString>(this.transaction.Signature), this.GetRawHash().Hash, approves);
                if (approves.Contains(ByteString.CopyFrom(address)))
                {
                    throw new PermissionException(Wallet.AddressToBase58(address) + "had signed!");
                }
            }

            long weight = GetWeight(permission, address);
            if (weight == 0)
            {
                throw new PermissionException(
                    privatekey.ToHexString() + " address is " +
                    Wallet.AddressToBase58(address) + "but it is not contained of permission.");
            }

            ECDSASignature signature = ec_key.Sign(this.GetRawHash().Hash);
            this.transaction.Signature.Add(ByteString.CopyFrom(signature.ToByteArray()));
        }

        public static byte[] GetOwner(Transaction.Types.Contract contract)
        {
            ByteString owner;
            try
            {
                Any contractParameter = contract.Parameter;
                switch (contract.Type)
                {
                    case Transaction.Types.Contract.Types.ContractType.AccountCreateContract:
                        owner = contractParameter.Unpack<AccountCreateContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.TransferContract:
                        owner = contractParameter.Unpack<TransferContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.TransferAssetContract:
                        owner = contractParameter.Unpack<TransferAssetContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.VoteAssetContract:
                        owner = contractParameter.Unpack<VoteAssetContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.VoteWitnessContract:
                        owner = contractParameter.Unpack<VoteWitnessContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.WitnessCreateContract:
                        owner = contractParameter.Unpack<WitnessCreateContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.AssetIssueContract:
                        owner = contractParameter.Unpack<AssetIssueContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.WitnessUpdateContract:
                        owner = contractParameter.Unpack<WitnessUpdateContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ParticipateAssetIssueContract:
                        owner = contractParameter.Unpack<ParticipateAssetIssueContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.AccountUpdateContract:
                        owner = contractParameter.Unpack<AccountUpdateContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.FreezeBalanceContract:
                        owner = contractParameter.Unpack<FreezeBalanceContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.UnfreezeBalanceContract:
                        owner = contractParameter.Unpack<UnfreezeBalanceContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.UnfreezeAssetContract:
                        owner = contractParameter.Unpack<UnfreezeAssetContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.WithdrawBalanceContract:
                        owner = contractParameter.Unpack<WithdrawBalanceContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.CreateSmartContract:
                        owner = contractParameter.Unpack<CreateSmartContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.TriggerSmartContract:
                        owner = contractParameter.Unpack<TriggerSmartContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.UpdateAssetContract:
                        owner = contractParameter.Unpack<UpdateAssetContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ProposalCreateContract:
                        owner = contractParameter.Unpack<ProposalCreateContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ProposalApproveContract:
                        owner = contractParameter.Unpack<ProposalApproveContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ProposalDeleteContract:
                        owner = contractParameter.Unpack<ProposalDeleteContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.SetAccountIdContract:
                        owner = contractParameter.Unpack<SetAccountIdContract>().OwnerAddress;
                        break;
                    //case Transaction.Types.Contract.Types.ContractType.BuyStorageContract:
                    //  owner = contractParameter.Unpack<BuyStorageContract>().OwnerAddress;
                    //  break;
                    //case Transaction.Types.Contract.Types.ContractType.BuyStorageBytesContract:
                    //  owner = contractParameter.Unpack<BuyStorageBytesContract>().OwnerAddress;
                    //  break;
                    //case Transaction.Types.Contract.Types.ContractType.SellStorageContract:
                    //  owner = contractParameter.Unpack<SellStorageContract>().OwnerAddress;
                    //  break;
                    case Transaction.Types.Contract.Types.ContractType.UpdateSettingContract:
                        owner = contractParameter.Unpack<UpdateSettingContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.UpdateEnergyLimitContract:
                        owner = contractParameter.Unpack<UpdateEnergyLimitContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ClearAbicontract:
                        owner = contractParameter.Unpack<ClearABIContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ExchangeCreateContract:
                        owner = contractParameter.Unpack<ExchangeCreateContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ExchangeInjectContract:
                        owner = contractParameter.Unpack<ExchangeInjectContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ExchangeWithdrawContract:
                        owner = contractParameter.Unpack<ExchangeWithdrawContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.ExchangeTransactionContract:
                        owner = contractParameter.Unpack<ExchangeTransactionContract>().OwnerAddress;
                        break;
                    case Transaction.Types.Contract.Types.ContractType.AccountPermissionUpdateContract:
                        owner = contractParameter.Unpack<AccountPermissionUpdateContract>().OwnerAddress;
                        break;
                    default:
                        return null;
                }
                return owner.ToByteArray();
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }

        public static void ValidContractProto(List<Transaction> transactions)
        {
            List<Task<bool>> tasks = new List<Task<bool>>();
            transactions.ForEach(tx =>
            {
                Task<bool> task = Task.Run(() =>
                {
                    try
                    {
                        ValidContractProto(tx.RawData.Contract[0]);
                        return true;
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error(e.Message);
                    }
                    return false;
                });
                tasks.Add(task);
            });

            foreach (Task<bool> task in tasks)
            {
                if (!task.Result)
                {
                    throw new P2pException(
                        P2pException.ErrorType.PROTOBUF_ERROR,
                        P2pException.GetDescription(P2pException.ErrorType.PROTOBUF_ERROR));
                }
            }
        }

        public static void ValidContractProto(Transaction.Types.Contract contract)
        {
            System.Type type = null;
            Any contract_parameter = contract.Parameter;
            Google.Protobuf.IMessage src = null;
            Google.Protobuf.IMessage contract_message = null;

            switch (contract.Type)
            {
                case ContractType.AccountCreateContract:
                    src = contract_parameter.Unpack<AccountCreateContract>();
                    contract_message = Parse<AccountCreateContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.TransferContract:
                    src = contract_parameter.Unpack<TransferContract>();
                    contract_message = Parse<TransferContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.TransferAssetContract:
                    src = contract_parameter.Unpack<TransferAssetContract>();
                    contract_message = Parse<TransferAssetContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.VoteAssetContract:
                    src = contract_parameter.Unpack<VoteAssetContract>();
                    contract_message = Parse<VoteAssetContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.VoteWitnessContract:
                    src = contract_parameter.Unpack<VoteWitnessContract>();
                    contract_message = Parse<VoteWitnessContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.WitnessCreateContract:
                    src = contract_parameter.Unpack<WitnessCreateContract>();
                    contract_message = Parse<WitnessCreateContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.AssetIssueContract:
                    src = contract_parameter.Unpack<AssetIssueContract>();
                    contract_message = Parse<AssetIssueContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.WitnessUpdateContract:
                    src = contract_parameter.Unpack<WitnessUpdateContract>();
                    contract_message = Parse<WitnessUpdateContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ParticipateAssetIssueContract:
                    src = contract_parameter.Unpack<ParticipateAssetIssueContract>();
                    contract_message = Parse<ParticipateAssetIssueContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.AccountUpdateContract:
                    src = contract_parameter.Unpack<AccountUpdateContract>();
                    contract_message = Parse<AccountUpdateContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.FreezeBalanceContract:
                    src = contract_parameter.Unpack<FreezeBalanceContract>();
                    contract_message = Parse<FreezeBalanceContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.UnfreezeBalanceContract:
                    src = contract_parameter.Unpack<UnfreezeBalanceContract>();
                    contract_message = Parse<UnfreezeBalanceContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.UnfreezeAssetContract:
                    src = contract_parameter.Unpack<UnfreezeAssetContract>();
                    contract_message = Parse<UnfreezeAssetContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.WithdrawBalanceContract:
                    src = contract_parameter.Unpack<WithdrawBalanceContract>();
                    contract_message = Parse<WithdrawBalanceContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.CreateSmartContract:
                    src = contract_parameter.Unpack<CreateSmartContract>();
                    contract_message = Parse<CreateSmartContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.TriggerSmartContract:
                    src = contract_parameter.Unpack<TriggerSmartContract>();
                    contract_message = Parse<TriggerSmartContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.UpdateAssetContract:
                    src = contract_parameter.Unpack<UpdateAssetContract>();
                    contract_message = Parse<UpdateAssetContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ProposalCreateContract:
                    src = contract_parameter.Unpack<ProposalCreateContract>();
                    contract_message = Parse<ProposalCreateContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ProposalApproveContract:
                    src = contract_parameter.Unpack<ProposalApproveContract>();
                    contract_message = Parse<ProposalApproveContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ProposalDeleteContract:
                    src = contract_parameter.Unpack<ProposalDeleteContract>();
                    contract_message = Parse<ProposalDeleteContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.SetAccountIdContract:
                    src = contract_parameter.Unpack<SetAccountIdContract>();
                    contract_message = Parse<SetAccountIdContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.UpdateSettingContract:
                    src = contract_parameter.Unpack<UpdateSettingContract>();
                    contract_message = Parse<UpdateSettingContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.UpdateEnergyLimitContract:
                    src = contract_parameter.Unpack<UpdateEnergyLimitContract>();
                    contract_message = Parse<UpdateEnergyLimitContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ClearAbicontract:
                    src = contract_parameter.Unpack<ClearABIContract>();
                    contract_message = Parse<ClearABIContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ExchangeCreateContract:
                    src = contract_parameter.Unpack<ExchangeCreateContract>();
                    contract_message = Parse<ExchangeCreateContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ExchangeInjectContract:
                    src = contract_parameter.Unpack<ExchangeInjectContract>();
                    contract_message = Parse<ExchangeInjectContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ExchangeWithdrawContract:
                    src = contract_parameter.Unpack<ExchangeWithdrawContract>();
                    contract_message = Parse<ExchangeWithdrawContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.ExchangeTransactionContract:
                    src = contract_parameter.Unpack<ExchangeTransactionContract>();
                    contract_message = Parse<ExchangeTransactionContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                case ContractType.AccountPermissionUpdateContract:
                    src = contract_parameter.Unpack<AccountPermissionUpdateContract>();
                    contract_message = Parse<AccountPermissionUpdateContract>(Message.GetCodedInputStream(src.ToByteArray()));
                    break;
                default:
                    break;
            }

            if (type == null)
            {
                throw new P2pException(P2pException.ErrorType.PROTOBUF_ERROR, P2pException.GetDescription(P2pException.ErrorType.PROTOBUF_ERROR));
            }

            Message.CompareBytes(src.ToByteArray(), contract_message.ToByteArray());
        }

        public static T Parse<T>(CodedInputStream stream)
            where T : Google.Protobuf.IMessage
        {
            T instance = (T)Activator.CreateInstance(typeof(T));
            instance.MergeFrom(stream);
            return instance;
        }

        public static byte[] ToAddress(Transaction.Types.Contract contract)
        {
            ByteString result = null;
            try
            {
                Any contract_parameter = contract.Parameter;
                switch (contract.Type)
                {
                    case ContractType.TransferContract:
                        result = contract_parameter.Unpack<TransferContract>().ToAddress;
                        break;
                    case ContractType.TransferAssetContract:
                        result = contract_parameter.Unpack<TransferAssetContract>().ToAddress;
                        break;
                    case ContractType.ParticipateAssetIssueContract:
                        result = contract_parameter.Unpack<ParticipateAssetIssueContract>().ToAddress;
                        break;
                }
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }

            return result?.ToByteArray();
        }

        public static long GetCallValue(Transaction.Types.Contract contract)
        {
            long result = 0;
            try
            {
                Any contract_parameter = contract.Parameter;
                switch (contract.Type)
                {
                    case ContractType.TriggerSmartContract:
                        result = contract_parameter.Unpack<TriggerSmartContract>().CallValue;
                        break;
                    case ContractType.CreateSmartContract:
                        result = contract_parameter.Unpack<CreateSmartContract>().NewContract.CallValue;
                        break;
                }
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        public static long GetCallTokenValue(Transaction.Types.Contract contract)
        {
            long result = 0;
            try
            {
                Any contract_parameter = contract.Parameter;
                switch (contract.Type)
                {
                    case ContractType.TriggerSmartContract:
                        result = contract_parameter.Unpack<TriggerSmartContract>().CallTokenValue;
                        break;
                    case ContractType.CreateSmartContract:
                        result = contract_parameter.Unpack<CreateSmartContract>().CallTokenValue;
                        break;
                }
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        public static bool ValidateSignature(Transaction tx, byte[] hash, DatabaseManager db_manager)
        {
            Permission permission = null;
            AccountStore account_store = db_manager.Account;
            Transaction.Types.Contract contract = tx.RawData.Contract?[0];

            int permission_id = contract.PermissionId;
            byte[] owner = GetOwner(contract);
            AccountCapsule account = account_store.Get(owner);

            if (account == null)
            {
                if (permission_id == 0)
                    permission = AccountCapsule.GetDefaultPermission(ByteString.CopyFrom(owner));
                else if (permission_id == 2)
                    permission = AccountCapsule.CreateDefaultActivePermission(ByteString.CopyFrom(owner), db_manager);
            }
            else
            {
                permission = account.GetPermissionById(permission_id);
            }

            if (permission == null)
                throw new PermissionException("Permission is not exist");

            if (permission_id != 0)
            {
                if (permission.Type != Permission.Types.PermissionType.Active)
                    throw new PermissionException("Permission type is error");

                if (!Wallet.CheckPermissionOperations(permission, contract))
                    throw new PermissionException("Invalid Permission");
            }

            return CheckWeight(permission, new List<ByteString>(tx.Signature), hash, null) >= permission.Threshold;
        }

        public bool ValidateSignature(DatabaseManager db_manager)
        {
            if (this.is_verifyed)
                return true;

            if (this.transaction.Signature.Count <= 0 || this.transaction.RawData.Contract.Count <= 0)
            {
                throw new ValidateSignatureException("Invalid signature or contract");
            }

            if (this.transaction.Signature.Count > db_manager.DynamicProperties.GetTotalSignNum())
            {
                throw new ValidateSignatureException("Too many signatures");
            }

            byte[] hash = this.GetRawHash().Hash;

            try
            {
                if (!ValidateSignature(this.transaction, hash, db_manager))
                {
                    this.is_verifyed = false;
                    throw new ValidateSignatureException("Invalid signature");
                }
            }
            catch (System.Exception e)
            {
                this.is_verifyed = false;
                throw new ValidateSignatureException(e.Message);
            }

            return this.is_verifyed = true;
        }

        public void SetResultCode(Transaction.Types.Result.Types.contractResult contract_result)
        {
            Transaction.Types.Result result = new Transaction.Types.Result();
            result.ContractRet = contract_result;
            if (this.transaction.Ret.Count > 0)
            {
                this.transaction.Ret[0].ContractRet = contract_result;
                return;
            }

            this.transaction.Ret.Add(result);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("TransactionCapsule \n[ ");
            builder.Append("hash=").Append(this.GetRawHash()).Append("\n");
            int i = 0;

            if (this.transaction.RawData.Contract.Count > 0)
            {
                builder.Append("contract list:{ ");
                foreach (Transaction.Types.Contract contract in this.transaction.RawData.Contract)
                {
                    builder.Append("[" + i + "] ").Append("type: ").Append(contract.Type).Append("\n");
                    builder.Append("from address=").Append(GetOwner(contract)).Append("\n");
                    builder.Append("to address=").Append(ToAddress(contract)).Append("\n");

                    if (contract.Type.Equals(ContractType.TransferContract))
                    {
                        TransferContract transferContract;
                        try
                        {
                            transferContract = contract.Parameter.Unpack<TransferContract>();
                            builder.Append("transfer amount=").Append(transferContract.Amount).Append("\n");
                        }
                        catch (InvalidProtocolBufferException e)
                        {
                            Logger.Warning(e.StackTrace);
                        }
                    }
                    else if (contract.Type.Equals(ContractType.TransferAssetContract))
                    {
                        TransferAssetContract transferAssetContract;
                        try
                        {
                            transferAssetContract = contract.Parameter.Unpack<TransferAssetContract>();
                            builder.Append("transfer asset=").Append(transferAssetContract.AssetName).Append("\n");
                            builder.Append("transfer amount=").Append(transferAssetContract.Amount).Append("\n");
                        }
                        catch (InvalidProtocolBufferException e)
                        {
                            Logger.Warning(e.StackTrace);
                        }
                    }
                    if (this.transaction.Signature.Count >= i + 1)
                    {
                        Interlocked.Increment(ref i);
                        builder.Append("sign=")
                            .Append(this.transaction.Signature[i].ToBase64())
                            .Append("\n");
                    }
                }
                builder.Append("}\n");
            }
            else
            {
                builder.Append("contract list is empty\n");
            }
            builder.Append("]");

            return builder.ToString();
        }
        #endregion
    }
}
