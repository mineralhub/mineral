using MineralCLI.Network;
using MineralCLI.Util;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace MineralCLI.Commands
{
    public class NodeCommand : BaseCommand
    {
        /// <summary>
        /// Get information node list
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// </param>
        /// <returns></returns>
        public static bool ListNode(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] \n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.ListNode(out NodeList nodes);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintNodeList(nodes));
                }

                OutputResultMessage(command, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
