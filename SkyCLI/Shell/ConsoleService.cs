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
            { RpcCommands.Block.GetBlock, new CommandHandler(BlockCommand.OnGetBlock) },
            { RpcCommands.Block.GetBlockHash, new CommandHandler(BlockCommand.OnGetBlockHash) },
            { RpcCommands.Block.GetHeight, new CommandHandler(BlockCommand.OnGetHeight) },
            { RpcCommands.Block.GetCurrentBlockHash, new CommandHandler(BlockCommand.OnGetCurrentBlockHash) },
            { RpcCommands.Block.GetTransaction, new CommandHandler(BlockCommand.OnGetTransaction) },

            // Node
            { RpcCommands.Node.NodeList, new CommandHandler(NodeCommand.OnNodeList) },

            // Wallet
            { RpcCommands.Wallet.CreateAccount, new CommandHandler(WalletCommand.OnCreateAccount) },
            { RpcCommands.Wallet.OpenAccount, new CommandHandler(WalletCommand.OnOpenAccount) },
            { RpcCommands.Wallet.CloseAccount, new CommandHandler(WalletCommand.OnCloseAccount) },
            { RpcCommands.Wallet.GetAccount, new CommandHandler(WalletCommand.OnGetAccount) },
            { RpcCommands.Wallet.GetAddress, new CommandHandler(WalletCommand.OnGetAddress) },
            { RpcCommands.Wallet.GetBalance, new CommandHandler(WalletCommand.OnGetBalance) },
            { RpcCommands.Wallet.SendTo, new CommandHandler(WalletCommand.OnSendTo) },
            { RpcCommands.Wallet.FreezeBalance, new CommandHandler(WalletCommand.OnFreezeBalance) },
            { RpcCommands.Wallet.UnfreezeBalance, new CommandHandler(WalletCommand.OnUnfreezeBalance) },
            { RpcCommands.Wallet.VoteWitness, new CommandHandler(WalletCommand.OnVoteWitness) },
        };

        public override bool OnCommand(string[] parameters)
        {
            return commands.ContainsKey(parameters[0]) ? commands[parameters[0]](parameters) : base.OnCommand(parameters);
        }

        public void Dispose()
        {
            commands.Clear();
            base.Dispose();
        }
    }
}
