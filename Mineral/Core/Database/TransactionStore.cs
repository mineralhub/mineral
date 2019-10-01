using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Database
{
    public class TransactionStore : MineralStoreWithRevoking<TransactionCapsule, Transaction>
    {
        #region Field
        private BlockStore block_store;
        private KhaosDatabase khaos_database;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public TransactionStore(IRevokingDatabase revoking_database, BlockStore block, KhaosDatabase khaos, string db_name = "transaction")
            : base(revoking_database, db_name)
        {
            this.block_store = block;
            this.khaos_database = khaos;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private TransactionCapsule GetTransactionFromBlockStore(byte[] key, long block_num)
        {
            List<BlockCapsule> blocks = this.block_store.GetLimitNumber(block_num, 1);
            if (blocks.Count != 0)
            {
                foreach (TransactionCapsule tx in blocks[0].Transactions)
                {
                    if (tx.Id.Equals(SHA256Hash.Wrap(key)))
                    {
                        return tx;
                    }
                }
            }
            return null;
        }

        private TransactionCapsule GetTransactionFromKhaosDatabase(byte[] key, long high)
        {
            List<KhaosBlock> blocks = this.khaos_database.MiniStore.GetBlockByNum(high);
            foreach (KhaosBlock kblock in blocks)
            {
                foreach (TransactionCapsule tx in kblock.Block.Transactions)
                {
                    if (tx.Id.Equals(SHA256Hash.Wrap(key)))
                    {
                        return tx;
                    }
                }
            }
            return null;
        }

        private void DeleteIndex(byte[] key)
        {
            //if (Objects.nonNull(indexHelper))
            //{
            //    TransactionCapsule item;
            //    try
            //    {
            //        item = get(key);
            //        if (Objects.nonNull(item))
            //        {
            //            indexHelper.remove(item.getInstance());
            //        }
            //    }
            //    catch (StoreException e)
            //    {
            //        logger.error("deleteIndex: ", e);
            //    }
            //}
        }
        #endregion


        #region External Method
        public override void Put(byte[] key, TransactionCapsule item)
        {
            if (item == null || item.BlockNum == -1)
                base.Put(key, item);
            else
                this.revoking_db.Put(key, BitConverter.GetBytes(item.BlockNum));
        }

        public override TransactionCapsule Get(byte[] key)
        {
            TransactionCapsule transaction = null;
            byte[] value = this.revoking_db.GetUnchecked(key);

            if (value.IsNotNullOrEmpty())
            {
                if (value.Length == 8)
                {
                    long block_num = BitConverter.ToInt64(value, 0);
                    transaction = GetTransactionFromBlockStore(key, block_num);
                    if (transaction == null)
                    {
                        transaction = GetTransactionFromKhaosDatabase(key, block_num);
                    }
                }

                transaction = transaction ?? new TransactionCapsule(value);
            }

            return transaction;
        }

        public override TransactionCapsule GetUnchecked(byte[] key)
        {
            TransactionCapsule transaction = null;
            try
            {
                transaction = Get(key);
            }
            catch
            {
                transaction = null;
            }

            return transaction;
        }

        public override void Delete(byte[] key)
        {
            DeleteIndex(key);
            base.Delete(key);
        }
        #endregion
    }
}
