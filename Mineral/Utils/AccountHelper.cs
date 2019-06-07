using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Cryptography;

namespace Mineral.Utils
{
    public static class AccountHelper
    {
        public static string ToAddress(ECKey key)
        {
            return ToAddress(key.GetPubKey(false).ToArray());
        }

        public static string ToAddress(byte[] pubkey)
        {
            byte[] data = new byte[21];
            data[0] = Config.Instance.AddressVersion;
            Buffer.BlockCopy(pubkey.SHA256().RIPEMD160(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }

        public static UInt160 ToAddressHash(byte[] pubkey)
        {
            return new UInt160(pubkey.SHA256().RIPEMD160());
        }

        public static UInt160 ToAddressHash(string address)
        {
            byte[] data = address.Base58CheckDecode();
            if (data.Length != 21)
                throw new FormatException();
            if (data[0] != Config.Instance.AddressVersion)
                throw new FormatException();
            return new UInt160(data.Skip(1).ToArray());
        }

        public static bool IsAddress(string address)
        {
            try
            {
                byte[] data = address.Base58CheckDecode();
                if (data.Length != 21)
                    throw new FormatException("The address (" + address + ") must be 21 bytes");
                if (data[0] != Config.Instance.AddressVersion)
                    throw new FormatException();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ToAddress(UInt160 addressHash)
        {
            byte[] data = new byte[21];
            data[0] = Config.Instance.AddressVersion;
            Buffer.BlockCopy(addressHash.ToArray(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }
    }
}
