using Google.Protobuf;
using Mineral;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MineralCLI.Util
{
    public static class ProtocolUtil
    {
        public static Transaction SetExpirationTime(ref Transaction transaction)
        {
            transaction.RawData.Expiration = Helper.CurrentTimeMillis() + 6 * 60 * 60 * 1000;

            return transaction;
        }

        public static Transaction SetPermissionId(ref Transaction transaction)
        {
            if (transaction.Signature.Count != 0 ||
                transaction.RawData.Contract[0].PermissionId != 0)
            {
                return transaction;
            }

            Console.WriteLine("Your transaction details are as follows, please confirm.");
            Console.WriteLine("transaction hex string is " + transaction.ToByteArray().ToHexString());
            Console.WriteLine(PrintUtil.PrintTransaction(transaction));
            Console.WriteLine("Please confirm and input your permission id," +
                " if input y or Y means default 0, other non-numeric characters will cancell transaction.");

            int permission_id = InputPermissionid();
            if (permission_id < 0)
            {
                throw new OperationCanceledException("user cancelled.");
            }

            if (permission_id != 0)
            {
                transaction.RawData.Contract[0].PermissionId = permission_id;
            }

            return transaction;
        }

        public static int InputPermissionid()
        {
            string input = Console.ReadLine();

            try
            {
                string data = Regex.Split(input, @"\s+")[0];
                if (data == "y")
                {
                    return 0;
                }

                return int.Parse(data);

            }
            catch (System.Exception e)
            {
                return -1;
            }
        }
    }
}
