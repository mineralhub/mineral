using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Mineral.Cryptography;
using Mineral.Utils;

namespace Mineral.Core.Config.Arguments
{
    public class LocalWitness
    {
        #region Field
        private List<string> privatekeys = new List<string>();
        private byte[] witness_account_address = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public LocalWitness()
        {
        }

        public LocalWitness(string privatekey)
        {
            AddPrivateKeys(privatekey);
        }

        public LocalWitness(List<string> privatekeys)
        {
            SetPrivateKeys(privatekeys);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool IsValidate(string key)
        {
            if (key.StartsWith("0x"))
                key = key.Substring(2);

            if (!string.IsNullOrEmpty(key) && 
                key.Length != Parameter.ChainParameters.PRIVATE_KEY_LENGTH)
            {
                Logger.Warning("Private key [" + key + "] must be " + Parameter.ChainParameters.PRIVATE_KEY_LENGTH + "bits");
                return false;
            }

            return true;
        }
        #endregion


        #region External Method
        public void InitWitnessAccountAddress()
        {
            if (this.witness_account_address == null)
            {
                this.witness_account_address = new ECKey(GetPrivateKey().HexToBytes(), true).GetPublicAddress();
            }
        }

        public void SetWitnessAccountAddress(byte[] address)
        {
            this.witness_account_address = address;
        }

        public byte[] GetWitnessAccountAddress()
        {
            if (this.witness_account_address == null)
            {
                string privatekey = GetPrivateKey();
                if (!privatekey.IsNullOrEmpty())
                {
                    ECKey key = new ECKey(privatekey.HexToBytes(), true);
                    this.witness_account_address = key.GetPublicAddress();
                    return this.witness_account_address;
                }
            }

            return null;
        }

        public void AddPrivateKeys(string privatekey)
        {
            if (IsValidate(privatekey))
            {
                this.privatekeys.Add(privatekey);
            }
        }

        public void SetPrivateKeys(List<string> keys)
        {
            if (keys.IsNullOrEmpty()) return;

            foreach (string key in keys)
            {
                if (IsValidate(key)) return;
            }

            this.privatekeys = new List<string>(keys);
        }

        public string GetPrivateKey()
        {
            if (privatekeys.IsNullOrEmpty())
            {
                Logger.Warning("Private key is null");
                return null;
            }

            return privatekeys[0];
        }
        #endregion
    }
}
