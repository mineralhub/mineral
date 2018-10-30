using System;
using System.Collections.Generic;
using Sky.Network.RPC.Command;
using SkyCLI.Commands;
using static SkyCLI.Commands.BaseCommand;

namespace SkyCLI.Shell
{
    public class ConsoleService : ConsoleServiceBase, IDisposable
    {
        private Dictionary<string, CommandHandler> commands = new Dictionary<string, CommandHandler>()
        {
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
            { RpcCommand.Wallet.GetAccount, new CommandHandler(WalletCommand.OnGetAccount) },
            { RpcCommand.Wallet.GetAddress, new CommandHandler(WalletCommand.OnGetAddress) },
            { RpcCommand.Wallet.GetBalance, new CommandHandler(WalletCommand.OnGetBalance) },
            { RpcCommand.Wallet.SendTo, new CommandHandler(WalletCommand.OnSendTo) },
            { RpcCommand.Wallet.LockBalance, new CommandHandler(WalletCommand.OnLockBalance) },
            { RpcCommand.Wallet.UnlockBalance, new CommandHandler(WalletCommand.OnUnlockBalance) },
            { RpcCommand.Wallet.VoteWitness, new CommandHandler(WalletCommand.OnVoteWitness) },
        };

        public override bool OnCommand(string[] parameters)
        {
            return commands.ContainsKey(parameters[0]) ? commands[parameters[0]](parameters) : base.OnCommand(parameters);
        }

        public override void OnHelp(string[] parameters)
        {
            string message =
                Program.version
                + "\n" + "".PadLeft(2) + "COMMAND : "
                + "\n" + "".PadLeft(4) + "BLOCK : "
                + "\n" + "".PadLeft(6) + RpcCommand.Block.GetBlock
                + "\n" + "".PadLeft(6) + RpcCommand.Block.GetBlockHash
                + "\n" + "".PadLeft(6) + RpcCommand.Block.GetHeight
                + "\n" + "".PadLeft(6) + RpcCommand.Block.GetCurrentBlockHash
                + "\n" + "".PadLeft(6) + RpcCommand.Block.GetTransaction

                + "\n"
                + "\n" + "".PadLeft(4) + "NODE : "
                + "\n" + "".PadLeft(6) + RpcCommand.Node.NodeList

                + "\n"
                + "\n" + "".PadLeft(4) + "WALLET :"
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.CreateAccount
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.OpenAccount
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.CloseAccount
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.GetBalance
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.SendTo
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.LockBalance
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.UnlockBalance
                + "\n" + "".PadLeft(6) + RpcCommand.Wallet.VoteWitness

                + "\n"
                + "\n" + "".PadLeft(2) + "MISC OPTION :"
                + "\n" + "".PadLeft(6) + BaseCommand.HelpCommandOption.Help;

            Console.WriteLine(message);
        }

        public new void Dispose()
        {
            commands.Clear();
            base.Dispose();
        }
    }
}
