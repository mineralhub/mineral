using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky;
using Sky.Network.RPC.Command;
using SkyCLI.Network;

namespace SkyCLI.Commands
{
    public class BlockCommand : BaseCommand
    {
        public static bool OnGetBlock(string[] parameters)
        {
            string usage = string.Format(
                "\t{0} [command option] <block hash>\n"
                , RpcCommand.Block.GetBlock);
            string command_option = HelpCategory.Option_Help;

            if (parameters.Length == 1 || parameters.Length > 3)
            {
                OutputHelpMessage(usage, "", command_option, "");
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, "", command_option, "");
                    index++;
                    return true;
                }
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, index, parameters.Length - index));
            SendCommand(Config.BlockVersion, RpcCommand.Block.GetBlock, param);
            return true;
        }

        public static bool OnGetBlockHash(string[] parameters)
        {
            string usage = string.Format(
                "\t{0} [command option] <block hash>\n"
                , RpcCommand.Block.GetBlockHash);
            string command_option = HelpCategory.Option_Help;

            if (parameters.Length == 1 || parameters.Length > 3)
            {
                OutputHelpMessage(usage, "", command_option, "");
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, "", command_option, "");
                    index++;
                    return true;
                }
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, index, parameters.Length - index));
            SendCommand(Config.BlockVersion, RpcCommand.Block.GetBlockHash, param);
            return true;
        }

        public static bool OnGetHeight(string[] parameters)
        {
            string usage = string.Format(
                "\t{0} [command option]\n"
                , RpcCommand.Block.GetHeight);
            string command_option = HelpCategory.Option_Help;

            if (parameters.Length > 2)
            {
                OutputHelpMessage(usage, "", command_option, "");
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, "", command_option, "");
                    index++;
                    return true;
                }
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, index, parameters.Length - index));
            SendCommand(Config.BlockVersion, RpcCommand.Block.GetHeight, param);
            return true;
        }

        public static bool OnGetCurrentBlockHash(string[] parameters)
        {
            string usage = string.Format(
                "\t{0} [command option]\n"
                , RpcCommand.Block.GetCurrentBlockHash);
            string command_option = HelpCategory.Option_Help;

            if (parameters.Length > 2)
            {
                OutputHelpMessage(usage, "", command_option, "");
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, "", command_option, "");
                    index++;
                    return true;
                }
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, index, parameters.Length - index));
            SendCommand(Config.BlockVersion, RpcCommand.Block.GetCurrentBlockHash, param);
            return true;
        }

        public static bool OnGetTransaction(string[] parameters)
        {
            string usage = string.Format(
                "\t{0} [command option] <transaction hash>\n"
                , RpcCommand.Block.GetTransaction);
            string command_option = HelpCategory.Option_Help;

            if (parameters.Length == 1 || parameters.Length > 3)
            {
                OutputHelpMessage(usage, "", command_option, "");
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, "", command_option, "");
                    index++;
                    return true;
                }
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, index, parameters.Length - index));
            SendCommand(Config.BlockVersion, RpcCommand.Block.GetCurrentBlockHash, param);
            return true;
        }
    }
}
