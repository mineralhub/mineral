using System;
using System.Collections.Generic;
using System.Reflection;
using Mineral.CommandLine.Attributes;
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
                + "\n" + "".PadLeft(1) + "BLOCK COMMAND :"
                ;
            foreach (FieldInfo info in typeof(RpcCommand.Block).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(1) + "NODE COMMAND : "
                ;
            foreach (FieldInfo info in typeof(RpcCommand.Node).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(1) + "WALLET COMMAND :"
                ;
            foreach (FieldInfo info in typeof(RpcCommand.Wallet).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(0) + "MISC OPTION :"
                + "\n" + "".PadLeft(4) + BaseCommand.HelpCommandOption.Help;

            Console.WriteLine(message);
        }

        public new void Dispose()
        {
            commands.Clear();
            base.Dispose();
        }
    }
}
