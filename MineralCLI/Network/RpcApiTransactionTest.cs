using Google.Protobuf;
using Mineral;
using Mineral.CommandLine;
using Mineral.Common.Net.RPC;
using Mineral.Common.Utils;
using Mineral.Core;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Mineral.Core.Net.RpcHandler;
using Mineral.Cryptography;
using Mineral.Wallets.KeyStore;
using MineralCLI.Exception;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Mineral.Core.Capsule.BlockCapsule;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;

namespace MineralCLI.Network
{
    using ResponseCode = TransactionSignWeight.Types.Result.Types.response_code;

    // For transaction test
    public partial class RpcApi
    {
        public static RpcApiResult ProcessTransactionExtentionForTest(TransactionExtention transaction_extention,
                                                                      byte[] privatekey,
                                                                      AccountCapsule account,
                                                                      out Transaction transaction)
        {
            transaction = null;
            if (transaction_extention == null)
            {
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "Transaction is null");
            }

            Return ret = transaction_extention.Result;
            if (!ret.Result)
            {
                OutputTransactionErrorMessage((int)ret.Code, ret.Message.ToStringUtf8());
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "");
            }

            transaction = transaction_extention.Transaction;
            if (transaction == null || transaction.RawData.Contract.Count == 0)
            {
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "Transaction is null");
            }

            Console.WriteLine("Receive txid = " + transaction_extention.Txid.ToByteArray().ToHexString());
            Console.WriteLine("Transaction hex string is " + PrintUtil.PrintTransaction(transaction));
            Console.WriteLine(PrintUtil.PrintTransaction(transaction_extention));

            RpcApiResult result = SignatureTransactionForTest(ref transaction, privatekey, account);
            if (!result.Result)
            {
                return result;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult SignatureTransactionForTest(ref Transaction transaction, byte[] privatekey, AccountCapsule account)
        {
            if (transaction.RawData.Timestamp == 0)
            {
                transaction.RawData.Timestamp = Helper.CurrentTimeMillis();
            }

            ProtocolUtil.SetExpirationTime(ref transaction);

            try
            {
                while (true)
                {

                    ECKey key = ECKey.FromPrivateKey(privatekey);
                    ECDSASignature signature = key.Sign(SHA256Hash.ToHash(transaction.RawData.ToByteArray()));

                    transaction.Signature.Add(ByteString.CopyFrom(signature.ToByteArray()));

                    TransactionSignWeight weight = RpcApi.GetTransactionSignWeight(transaction, account);
                    if (weight.Result.Code == ResponseCode.EnoughPermission)
                    {
                        break;
                    }
                    else if (weight.Result.Code == ResponseCode.NotEnoughPermission)
                    {
                        Console.WriteLine("Current signWeight is:");
                        Console.WriteLine(PrintUtil.PrintTransactionSignWeight(weight));
                        Console.WriteLine("Please confirm if continue add signature enter y or Y, else any other");

                        if (!CommandLineUtil.Confirm())
                        {
                            throw new CancelException("User cancelled");
                        }
                        continue;
                    }

                    throw new CancelException(weight.Result.Message);
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult BroadcastTransactionForTest(Transaction transaction)
        {
            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                Return ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                int retry = 10;
                while (ret.Result == false && ret.Code == Return.Types.response_code.ServerBusy && retry > 0)
                {
                    retry--;
                    receive = SendCommand(RpcCommand.Transaction.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                    ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                    Console.WriteLine("Retry broadcast : " + (11 - retry));

                    Thread.Sleep(1000);
                }

                if (!ret.Result)
                {
                    Console.WriteLine("Code : " + ret.Code);
                    Console.WriteLine("Message : " + ret.Message.ToStringUtf8());
                    return new RpcApiResult(false, RpcMessage.INVALID_REQUEST, ret.Message.ToStringUtf8());
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static TransactionSignWeight GetTransactionSignWeight(Transaction transaction, AccountCapsule account)
        {
            TransactionSignWeight weight = new TransactionSignWeight();
            TransactionExtention extention = new TransactionExtention();
            weight.Result = new TransactionSignWeight.Types.Result();
            extention.Result = new Return();

            extention.Transaction = transaction;
            extention.Txid = ByteString.CopyFrom(SHA256Hash.ToHash(transaction.RawData.ToByteArray()));
            extention.Result.Result = true;
            extention.Result.Code = Return.Types.response_code.Success;

            weight.Transaction = extention;

            try
            {
                Contract contract = transaction.RawData.Contract[0];
                byte[] owner = TransactionCapsule.GetOwner(contract);

                int permission_id = contract.PermissionId;
                Permission permission = account.GetPermissionById(permission_id);
                if (permission == null)
                {
                    throw new PermissionException("permission isn't exit");
                }

                if (permission_id != 0)
                {
                    if (permission.Type != Permission.Types.PermissionType.Active)
                    {
                        throw new PermissionException("Permission type is error");
                    }

                    if (!CheckPermissionOprations(permission, contract))
                    {
                        throw new PermissionException("Permission denied");
                    }
                }

                weight.Permission = permission;
                if (transaction.Signature.Count > 0)
                {
                    List<ByteString> approves = new List<ByteString>();

                    weight.ApprovedList.AddRange(approves);
                    weight.CurrentWeight = TransactionCapsule.CheckWeight(permission,
                                                                          new List<ByteString>(transaction.Signature),
                                                                          SHA256Hash.ToHash(transaction.RawData.ToByteArray()),
                                                                          approves);
                }

                if (weight.CurrentWeight >= permission.Threshold)
                {
                    weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.EnoughPermission;
                }
                else
                {
                    weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.NotEnoughPermission;
                }
            }
            catch (SignatureFormatException e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.SignatureFormatError;
                weight.Result.Message = e.Message;
            }
            catch (SignatureException e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.ComputeAddressError;
                weight.Result.Message = e.Message;
            }
            catch (PermissionException e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.PermissionError;
                weight.Result.Message = e.Message;
            }
            catch (System.Exception e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.OtherError;
                weight.Result.Message = e.Message;
            }

            return weight;
        }

        public static TransactionExtention CreateTransactionExtention(IMessage message, ContractType type, BlockHeader header, BlockId id)
        {
            TransactionCapsule transaction = CreateTransactionCapsule(message, type, header, id);
            return CreateTransactionExtention(transaction);
        }

        public static TransactionCapsule CreateTransactionCapsule(IMessage message, ContractType type, BlockHeader header, BlockId id)
        {
            TransactionCapsule transaction = new TransactionCapsule(message, type);
            transaction.SetReference(id.Num, id.Hash);
            transaction.Expiration = header.RawData.Timestamp + 60000;
            transaction.Timestamp = Helper.CurrentTimeMillis();

            return transaction;
        }

        public static TransactionExtention CreateTransactionExtention(TransactionCapsule transaction)
        {
            TransactionExtention extention = new TransactionExtention();
            extention.Result = new Return();

            try
            {
                extention.Transaction = transaction.Instance;
                extention.Txid = ByteString.CopyFrom(transaction.Id.Hash);
                extention.Result.Result = true;
                extention.Result.Code = Return.Types.response_code.Success;

            }
            catch (ContractValidateException e)
            {
                extention.Result.Result = false;
                extention.Result.Code = Return.Types.response_code.ContractValidateError;
                extention.Result.Message = ByteString.CopyFromUtf8("Contract validate error " + e.Message);
                Logger.Debug(
                    string.Format("ContractValidateException: {0}", e.Message));
            }
            catch (System.Exception e)
            {
                extention.Result.Result = false;
                extention.Result.Code = Return.Types.response_code.ContractValidateError;
                extention.Result.Message = ByteString.CopyFromUtf8(e.Message);
                Logger.Debug("Exception caught" + e.Message);
            }

            return extention;
        }

        public static bool CheckPermissionOprations(Permission permission, Contract contract)
        {
            if (permission.Operations.Length != 32)
            {
                throw new PermissionException("operations size must 32");
            }

            return (permission.Operations[(int)contract.Type / 8] & (1 << ((int)contract.Type % 8))) != 0; ;
        }
    }
}
