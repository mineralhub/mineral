using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Overlay.Server;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using Mineral.Utils;
using static Mineral.Core.Capsule.BlockCapsule;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net
{
    public class MineralNetDelegate
    {
        #region Field
        private object locker_block = new object();
        private int block_id_cache_size = 100;

        private ConcurrentQueue<BlockId> fresh_block_id = new ConcurrentQueue<BlockId>();
        #endregion


        #region Property
        public List<PeerConnection> ActivePeers
        {
            get { return Manager.Instance.SyncPool.ActivePeers; }
        }

        public long SyncBeginNumber
        {
            get { return Manager.Instance.DBManager.GetSyncBeginNumber(); }
        }

        public BlockId GenesisBlockId
        {
            get { return Manager.Instance.DBManager.GenesisBlockId; }
        }

        public BlockId SolidBlockId
        {
            get { return Manager.Instance.DBManager.SolidBlockId; }
        }

        public BlockId HeadBlockId
        {
            get { return Manager.Instance.DBManager.HeadBlockId; }
        }

        public BlockCapsule GenesisBlock
        {
            get { return Manager.Instance.DBManager.GenesisBlock; }
        }

        public long HeadBlockTimeStamp
        {
            get { return Manager.Instance.DBManager.GetHeadBlockTimestamp(); }
        }

        public object LockBlock
        {
            get { return this.locker_block; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public bool CanChainRevoke(long num)
        {
            return num >= Manager.Instance.DBManager.GetSyncBeginNumber();
        }

        public bool Contain(SHA256Hash hash, MessageTypes type)
        {
            if (type.Equals(MessageTypes.MsgType.BLOCK))
            {
                return Manager.Instance.DBManager.ContainBlock(hash);
            }
            else if (type.Equals(MessageTypes.MsgType.TX))
            {
                return Manager.Instance.DBManager.Transaction.Contains(hash.Hash);
            }
            return false;
        }

        public bool ContainBlock(BlockId id)
        {
            return Manager.Instance.DBManager.ContainBlock(id);
        }

        public bool ContainBlockInMainChain(BlockId id)
        {
            return Manager.Instance.DBManager.ContainBlockInMainChain(id);
        }

        public Message GetData(SHA256Hash hash, InventoryType type)
        {
            Message result = null;
            try
            {
                switch (type)
                {
                    case InventoryType.Block:
                        {
                            result = new BlockMessage(Manager.Instance.DBManager.GetBlockById(hash));
                        }
                        break;
                    case InventoryType.Trx:
                        {
                            TransactionCapsule tx = Manager.Instance.DBManager.Transaction.Get(hash.Hash);
                            if (tx == null)
                                throw new StoreException();

                            result = new TransactionMessage(tx.Instance);
                        }
                        break;
                    default:
                        {
                            throw new StoreException();
                        }
                }
            }
            catch (StoreException e)
            {
                throw new P2pException(Exception.P2pException.ErrorType.DB_ITEM_NOT_FOUND,
                    "type: " + type + ", hash: " + hash.Hash.ToHexString());
            }

            return result;
        }

        public long GetBlockTime(BlockId id)
        {
            try
            {
                return Manager.Instance.DBManager.GetBlockById(id).Timestamp;
            }
            catch (ArgumentException)
            {
                throw new P2pException(P2pException.ErrorType.DB_ITEM_NOT_FOUND, id.GetString());
            }
            catch (ItemNotFoundException)
            {
                throw new P2pException(P2pException.ErrorType.DB_ITEM_NOT_FOUND, id.GetString());
            }
        }

        public BlockId GetBlockIdByNum(long num)
        {
            try
            {
                return Manager.Instance.DBManager.GetBlockIdByNum(num);
            }
            catch (ItemNotFoundException)
            {
                throw new P2pException(P2pException.ErrorType.DB_ITEM_NOT_FOUND, "num: " + num);
            }
        }

        public List<BlockId> GetBlockChainHashesOnFork(BlockId fork_hash)
        {
            try
            {
                return Manager.Instance.DBManager.GetBlockChainHashesOnFork(fork_hash);
            }
            catch (NonCommonBlockException)
            {
                throw new P2pException(Exception.P2pException.ErrorType.HARD_FORKED, fork_hash.GetString());
            }
        }

        public void AddFreshBlockId(BlockId id)
        {
            if (this.fresh_block_id.Count > this.block_id_cache_size)
            {
                this.fresh_block_id.TryDequeue(out _);
            }

            this.fresh_block_id.Enqueue(id);
        }

        public void ProcessBlock(BlockCapsule block)
        {
            lock (locker_block)
            {
                try
                {
                    if (!this.fresh_block_id.Contains(block.Id))
                    {
                        Manager.Instance.DBManager.PushBlock(block);
                        this.fresh_block_id.Enqueue(block.Id);
                        Logger.Info("Success process block " + block.Id.GetString());
                    }
                }
                catch (System.Exception e)
                {
                    if (e is ValidateSignatureException
                        || e is ContractValidateException
                        || e is ContractExeException
                        || e is UnLinkedBlockException
                        || e is ValidateScheduleException
                        || e is AccountResourceInsufficientException
                        || e is TaposException
                        || e is TooBigTransactionException
                        || e is TooBigTransactionResultException
                        || e is DupTransactionException
                        || e is TransactionExpirationException
                        || e is BadNumberBlockException
                        || e is BadBlockException
                        || e is NonCommonBlockException
                        || e is ReceiptCheckErrorException
                        || e is VMIllegalException)
                    {
                        throw new P2pException(P2pException.ErrorType.BAD_BLOCK, e.Message, e);
                    }

                    throw e;
                }
            }
        }

        public void PushTransaction(TransactionCapsule trx)
        {
            try
            {
                Manager.Instance.DBManager.PushTransaction(trx);
            }
            catch (System.Exception e)
            {
                if (e is ContractSizeNotEqualToOneException
                    || e is VMIllegalException)
                {
                    throw new P2pException(P2pException.ErrorType.BAD_TRX, e.Message, e);
                }
                else if (e is ContractValidateException
                        || e is ValidateSignatureException
                        || e is ContractExeException
                        || e is DupTransactionException
                        || e is TaposException
                        || e is TooBigTransactionException
                        || e is TransactionExpirationException
                        || e is ReceiptCheckErrorException
                        || e is TooBigTransactionResultException
                        || e is AccountResourceInsufficientException)
                {
                    throw new P2pException(P2pException.ErrorType.TRX_EXE_FAILED, e.Message, e);
                }
            }
        }


        public bool ValidBlock(BlockCapsule block)
        {
            bool result = false;

            try
            {
                if (!block.ValidateSignature(Manager.Instance.DBManager))
                {
                    return result;
                }

                foreach (WitnessCapsule witness in Manager.Instance.DBManager.Witness.GetAllWitnesses())
                {
                    if (witness.Address.Equals(block.WitnessAddress))
                    {
                        result = true;
                    }
                }
            }
            catch (ValidateSignatureException e)
            {
                throw new P2pException(P2pException.ErrorType.BAD_BLOCK, e.Message, e);
            }

            return result;
        }
        #endregion
    }
}


