using System;
using System.Collections.Generic;
using System.Text;
using Mineral.CommandLine;
using Mineral.Core;
using Mineral.Core.Capsule;
using Mineral.Wallets.KeyStore;
using MineralCLI.Network;
using Protocol;
using static Mineral.Core.Capsule.BlockCapsule;

namespace MineralCLI.Commands
{
    public class BenchMarkCommand : BaseCommand
    {
        public static bool BenchMarkTransaction(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <to address> <amount> <repeat> \n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            KeyStore keystore = RpcApi.SelectKeyStore();
            string password = CommandLineUtil.ReadPasswordString("Please input your password.");
            if (!KeyStoreService.DecryptKeyStore(password, keystore, out byte[] privatekey))
            {
                Console.WriteLine("Fail to decrypt keystore");
                return true;
            }

            byte[] owner_address = Wallet.Base58ToAddress(keystore.Address);
            byte[] to_address = Wallet.Base58ToAddress(parameters[0]);
            long amount = long.Parse(parameters[1]);
            int repeat = int.Parse(parameters[2]);

            Account out_account = null;
            AccountCapsule account = null;
            RpcApiResult result = RpcApi.GetBlockByLatestNum(out BlockExtention block);
            if (result.Result)
            {
                result = RpcApi.GetAccount(keystore.Address, out out_account);
            }

            TransferContract contract = null;
            if (result.Result)
            {
                account = new AccountCapsule(out_account);
                result = RpcApi.CreateTransaferContract(owner_address, to_address, amount, out contract);
            }

            List<Transaction> txs = new List<Transaction>();
            for (int i = 0; i < repeat; i++)
            {
                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    BlockHeader header = block.BlockHeader;
                    BlockId id = new BlockId(BlockId.Wrap(block.Blockid.ToByteArray()));
                    transaction_extention = RpcApi.CreateTransactionExtention(contract, Transaction.Types.Contract.Types.ContractType.TransferContract, header, id);
                    result = RpcApi.ProcessTransactionExtentionForTest(transaction_extention, privatekey, account, out Transaction tx);
                    txs.Add(tx);
                }
            }

            foreach (Transaction tx in txs)
            {
                RpcApi.BroadcastTransactionForTest(tx);
            }

            OutputResultMessage(command, result.Result, result.Code, result.Message);

            return true;
        }
    }
}
