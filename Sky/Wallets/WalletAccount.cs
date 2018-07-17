using Sky.Core;
using Sky.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sky.Wallets
{
    public class WalletAccount
    {
        public ECKey Key { get; private set; }
        public string Address => ToAddress(Key);
        public UInt160 AddressHash => ToAddressHash(Address);

        public WalletAccount(byte[] prikey)
        {
            try
            {
                Key = new ECKey(prikey, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool IsDelegate()
        {
            List<DelegatorState> dgs = Blockchain.Instance.GetDelegateStateAll();
            foreach (DelegatorState state in dgs)
            {
                if (state.AddressHash == AddressHash)
                    return true;
            }
            return false;
        }

        public static string ToAddress(ECKey key)
        {
            byte[] data = new byte[21];
            data[0] = Config.AddressVersion;
            Buffer.BlockCopy(key.PublicKey.ToByteArray(false).SHA256().RIPEMD160(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }

        public static UInt160 ToAddressHash(string address)
        {
            byte[] data = address.Base58CheckDecode();
            if (data.Length != 21)
                throw new FormatException();
            if (data[0] != Config.AddressVersion)
                throw new FormatException();
            return new UInt160(data.Skip(1).ToArray());
        }

        public static string ToAddress(UInt160 addressHash)
        {
            byte[] data = new byte[21];
            data[0] = Config.AddressVersion;
            Buffer.BlockCopy(addressHash.ToArray(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }

        public Fixed8 GetBalance()
        {
            AccountState state = Blockchain.Instance.GetAccountState(AddressHash);
            if (state == null)
                return Fixed8.Zero;
            return state.Balance;
        }
    }
}