using Google.Protobuf;
using Mineral;
using Mineral.CommandLine;
using Mineral.Common.Net.RPC;
using Mineral.Common.Utils;
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

namespace MineralCLI.Network
{
    using ResponseCode = TransactionSignWeight.Types.Result.Types.response_code;

    public partial class RpcApi
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static RpcApiResult CreateAccountContract(byte[] owner_address,
                                                         byte[] create_address,
                                                         out AccountCreateContract contract)
        {
            contract = new AccountCreateContract();
            contract.AccountAddress = ByteString.CopyFrom(create_address);
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateProposalContract(byte[] owner_address,
                                                          Dictionary<long, long> parameters,
                                                          out ProposalCreateContract contract)
        {
            contract = new ProposalCreateContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            foreach (var parameter in parameters)
            {
                contract.Parameters.Add(parameter.Key, parameter.Value);
            }

            return RpcApiResult.Success;
        }


        public static RpcApiResult CreateTransaferContract(byte[] owner_address,
                                                           byte[] to_address,
                                                           long amount, out TransferContract contract)
        {
            contract = new TransferContract();
            contract.ToAddress = ByteString.CopyFrom(to_address);
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.Amount = amount;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateTransferAssetContract(byte[] to_address,
                                                               byte[] from_address,
                                                               byte[] asset_name,
                                                               long amount,
                                                               out TransferAssetContract contract)
        {
            contract = new TransferAssetContract();
            contract.ToAddress = ByteString.CopyFrom(to_address);
            contract.AssetName = ByteString.CopyFrom(asset_name);
            contract.OwnerAddress = ByteString.CopyFrom(from_address);
            contract.Amount = amount;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateTransaction(IMessage contract,
                                                     string command_type,
                                                     out TransactionExtention transaction_extention)
        {
            try
            {
                transaction_extention = null;

                JObject receive = SendCommand(command_type, new JArray() { contract.ToByteArray() });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                transaction_extention = TransactionExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                Return ret = transaction_extention.Result;
                if (!ret.Result)
                {
                    OutputTransactionErrorMessage((int)ret.Code, ret.Message.ToStringUtf8());
                    return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "");
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateTransferAssetTransaction(TransferAssetContract contract,
                                                                  out TransactionExtention transaction_extention)
        {
            transaction_extention = null;

            JObject receive = SendCommand(RpcCommandType.TransferAsset, new JArray { contract.ToByteArray() });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            transaction_extention = TransactionExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            if (transaction_extention == null || !transaction_extention.Result.Result)
            {
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "Invalid transaction extention data");
            }

            if (transaction_extention.Transaction == null || transaction_extention.Transaction.RawData.Contract.Count == 0)
            {
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "Transaction is empty");
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult SignatureTransaction(ref Transaction transaction)
        {
            if (transaction.RawData.Timestamp == 0)
            {
                transaction.RawData.Timestamp = Helper.CurrentTimeMillis();
            }

            ProtocolUtil.SetExpirationTime(ref transaction);
            Console.WriteLine("Your transaction details are as follows, please confirm.");
            Console.WriteLine("transaction hex string is " + transaction.ToByteArray().ToHexString());
            Console.WriteLine(PrintUtil.PrintTransaction(transaction));
            Console.WriteLine(
                "Please confirm and input your permission id, if input y or Y means default 0, other non-numeric characters will cancell transaction.");
            ProtocolUtil.SetPermissionId(ref transaction);

            try
            {
                while (true)
                {
                    Console.WriteLine("Please choose keystore for signature.");
                    KeyStore key_store = SelectKeyStore();

                    string password = CommandLineUtil.ReadPasswordString("Please input password");
                    if (KeyStoreService.DecryptKeyStore(password, key_store, out byte[] privatekey))
                    {
                        ECKey key = ECKey.FromPrivateKey(privatekey);
                        ECDSASignature signature = key.Sign(SHA256Hash.ToHash(transaction.RawData.ToByteArray()));

                        transaction.Signature.Add(ByteString.CopyFrom(signature.ToByteArray()));
                        Console.WriteLine("current transaction hex string is " + transaction.ToByteArray().ToHexString());
                    }
                    else
                    {
                        return new RpcApiResult(false, RpcMessage.INVALID_PASSWORD, "Invalid keystore password");
                    }

                    JObject receive = SendCommand(RpcCommandType.GetTransactionSignWeight, new JArray() { transaction.ToByteArray() });

                    TransactionSignWeight weight = TransactionSignWeight.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
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

        public static RpcApiResult ProcessTransactionExtention(TransactionExtention transaction_extention)
        {
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

            Transaction transaction = transaction_extention.Transaction;
            if (transaction == null || transaction.RawData.Contract.Count == 0)
            {
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "Transaction is null");
            }

            Console.WriteLine("Receive txid = " + transaction_extention.Txid.ToByteArray().ToHexString());
            Console.WriteLine("Transaction hex string is " + PrintUtil.PrintTransaction(transaction));
            Console.WriteLine(PrintUtil.PrintTransaction(transaction_extention));

            RpcApiResult result = SignatureTransaction(ref transaction);
            if (!result.Result)
            {
                return result;
            }

            return BroadcastTransaction(transaction);
        }

        public static RpcApiResult BroadcastTransaction(Transaction transaction)
        {
            try
            {
                JObject receive = SendCommand(RpcCommandType.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                Return ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                int retry = 10;
                while (ret.Result == false && ret.Code == Return.Types.response_code.ServerBusy && retry > 0)
                {
                    retry--;
                    receive = SendCommand(RpcCommandType.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                    ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                    Console.WriteLine("Retry broadcast : " + (11 - retry));

                    Thread.Sleep(1000);
                }

                if (!ret.Result)
                {
                    Console.WriteLine("Code : " + ret.Code);
                    Console.WriteLine("Message : " + ret.Message);
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }
        #endregion
    }
}
