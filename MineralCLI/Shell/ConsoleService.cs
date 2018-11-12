using System;
using System.Collections.Generic;
using Mineral.Network.RPC.Command;
using MineralCLI.Commands;
using static MineralCLI.Commands.BaseCommand;

namespace MineralCLI.Shell
{
    public class ConsoleService : ConsoleServiceBase, IDisposable
    {
        private Dictionary<string, CommandHandler> commands = new Dictionary<string, CommandHandler>()
        {
            // General
            { RpcCommand.General.GetConfig, new CommandHandler(GeneralCommand.OnGetConfig) },

            // Block
            { RpcCommand.Block.GetBlock, new CommandHandler(BlockCommand.OnGetBlock) },
            { RpcCommand.Block.GetBlockHash, new CommandHandler(BlockCommand.OnGetBlockHash) },
            { RpcCommand.Block.GetHeight, new CommandHandler(BlockCommand.OnGetHeight) },
            { RpcCommand.Block.GetCurrentBlockHash, new CommandHandler(BlockCommand.OnGetCurrentBlockHash) },
            { RpcCommand.Block.GetTransaction, new CommandHandler(BlockCommand.OnGetTransaction) },

            // Node
            { RpcCommand.Node.NodeList, new CommandHandler(NodeCommand.OnNodeList) },

            // Wallet
            { RpcCommand.Wallet.CreateAccount, new CommandHandler(WalletCommand.OnCreateAccount) },
            { RpcCommand.Wallet.OpenAccount, new CommandHandler(WalletCommand.OnOpenAccount) },
            { RpcCommand.Wallet.CloseAccount, new CommandHandler(WalletCommand.OnCloseAccount) },
            { RpcCommand.Wallet.BackupAccount, new CommandHandler(WalletCommand.OnBackupAccount) },
            { RpcCommand.Wallet.GetAccount, new CommandHandler(WalletCommand.OnGetAccount) },
            { RpcCommand.Wallet.GetAddress, new CommandHandler(WalletCommand.OnGetAddress) },
            { RpcCommand.Wallet.GetBalance, new CommandHandler(WalletCommand.OnGetBalance) },
            { RpcCommand.Wallet.SendTo, new CommandHandler(WalletCommand.OnSendTo) },
            { RpcCommand.Wallet.LockBalance, new CommandHandler(WalletCommand.OnLockBalance) },
            { RpcCommand.Wallet.UnlockBalance, new CommandHandler(WalletCommand.OnUnlockBalance) },
            { RpcCommand.Wallet.VoteWitness, new CommandHandler(WalletCommand.OnVoteWitness) },
            { RpcCommand.Wallet.GetVoteWitness, new CommandHandler(WalletCommand.OnGetVoteWitness) },
        };

        public override bool OnCommand(string[] parameters)
        {
            return commands.ContainsKey(parameters[0]) ? commands[parameters[0]](parameters) : base.OnCommand(parameters);
        }

        public override void OnHelp(string[] parameters)
        {
            string message =
                Config.Instance.GetVersion()

                + "\n"
                + "\n" + "".PadLeft(0) + "COMMAND : "
                + "\n" + "".PadLeft(4) + "BLOCK COMMAND :"
                + "\n" + "".PadLeft(8) + RpcCommand.Block.GetBlock
                + "\n" + "".PadLeft(8) + RpcCommand.Block.GetBlockHash
                + "\n" + "".PadLeft(8) + RpcCommand.Block.GetHeight
                + "\n" + "".PadLeft(8) + RpcCommand.Block.GetCurrentBlockHash
                + "\n" + "".PadLeft(8) + RpcCommand.Block.GetTransaction

                + "\n"
                + "\n" + "".PadLeft(4) + "NODE COMMAND : "
                + "\n" + "".PadLeft(8) + RpcCommand.Node.NodeList

                + "\n"
                + "\n" + "".PadLeft(4) + "WALLET COMMAND :"
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.CreateAccount
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.OpenAccount
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.CloseAccount
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.BackupAccount
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.GetBalance
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.SendTo
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.LockBalance
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.UnlockBalance
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.VoteWitness
                + "\n" + "".PadLeft(8) + RpcCommand.Wallet.GetVoteWitness

                + "\n"
                + "\n" + "".PadLeft(0) + "MISC OPTION :"
                + "\n" + "".PadLeft(8) + BaseCommand.HelpCommandOption.Help;

            Console.WriteLine(message);
        }

        public new void Dispose()
        {
            commands.Clear();
            base.Dispose();
        }
    }
}
