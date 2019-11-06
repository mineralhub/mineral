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
        private List<byte[]> privatekeys = new List<byte[]>();
        private byte[] witness_account_address = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public LocalWitness()
        {
        }

        public LocalWitness(byte[] privatekey)
        {
            AddPrivateKeys(privatekey);
        }

        public LocalWitness(List<byte[]> privatekeys)
        {
            SetPrivateKeys(privatekeys);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool IsValidate(byte[] key)
        {
            if (key.IsNotNullOrEmpty()
                && key.Length != Parameter.ChainParameters.PRIVATE_KEY_BYTE_LENGTH)
            {
                Logger.Warning("Private key [" + key + "] must be " + Parameter.ChainParameters.PRIVATE_KEY_BYTE_LENGTH + "bits");
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
                ECKey key = ECKey.FromPrivateKey(GetPrivateKey());
                this.witness_account_address = Wallet.PublickKeyToAddress(key.PublicKey);
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
                byte[] privatekey = GetPrivateKey();
                if (privatekey.IsNotNullOrEmpty())
                {
                    ECKey key = ECKey.FromPrivateKey(privatekey);
                    this.witness_account_address = Wallet.PublickKeyToAddress(key.PublicKey);
                }
            }

            return this.witness_account_address;
        }

        public void AddPrivateKeys(byte[] privatekey)
        {
            if (IsValidate(privatekey))
            {
                this.privatekeys.Add(privatekey);
            }
        }

        public void SetPrivateKeys(List<byte[]> keys)
        {
            if (keys.IsNullOrEmpty()) return;

            foreach (byte[] key in keys)
            {
                if (!IsValidate(key)) return;
            }

            this.privatekeys = new List<byte[]>(keys);
        }

        public byte[] GetPrivateKey()
        {
            if (this.privatekeys.IsNullOrEmpty())
            {
                Logger.Warning("Private key is null");
                return null;
            }

            return privatekeys[0];
        }
        #endregion
    }
}
