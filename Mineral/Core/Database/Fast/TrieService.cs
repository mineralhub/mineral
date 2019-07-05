﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Database.Fast.Callback.StoreTrie;
using Mineral.Cryptography;

namespace Mineral.Core.Database.Fast
{
    public class TrieService
    {
        #region Field
        private DataBaseManager db_manager;
        #endregion


        #region Property
        public DataBaseManager DBManager
        {
            get { return this.db_manager; }
            set { this.db_manager = value; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] GetFullAccountStateRootHash()
        {
            long latest_number = this.db_manager.DynamicProperties.GetLatestBlockHeaderNumber();
            return GetAccountStateRootHash(latest_number);
        }

        public byte[] GetSolidityAccountStateRootHash()
        {
            long latest_number = this.db_manager.DynamicProperties.GetLatestSolidifiedBlockNum();
            return GetAccountStateRootHash(latest_number);
        }

        public byte[] GetAccountStateRootHash(long latest_number)
        {
            byte[] root_hash = null;
            try
            {
                BlockCapsule block = this.db_manager.GetBlockByNum(latest_number);
                ByteString value = block.Instance.BlockHeader.RawData.AccountStateRoot;

                root_hash = value == null ? null : value.ToByteArray();
                if (root_hash.SequenceEqual(new byte[0]))
                {
                    root_hash = Hash.EMPTY_TRIE_HASH;
                }
            }
            catch (System.Exception e)
            {
                Logger.Error(string.Format("Get the {0} block error, {1}", latest_number, e.Message));
            }

            return root_hash;
        }
        #endregion
    }
}
