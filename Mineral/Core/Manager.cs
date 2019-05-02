using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mineral.Core.State;
using Mineral.Database.BlockChain;
using Mineral.Utils;

namespace Mineral.Core
{
    public class Manager
    {
        #region Field
        private LevelDBBlock _fork_db = new LevelDBBlock(".output-fork-database");
        private LevelDBBlockChain _blockChain = new LevelDBBlockChain("./output-database");
        private LevelDBWalletIndexer _walletIndexer = new LevelDBWalletIndexer("./output-wallet-index");
        private LevelDBProperty _properties = new LevelDBProperty("./output-property");
        private CacheBlocks _cacheBlocks = null;

        private const uint _defaultCacheCapacity = 200000;
        #endregion


        #region Property
        internal LevelDBBlock ForkDB { get { return _fork_db; } }
        internal LevelDBBlockChain BlockChain { get { return _blockChain; } }
        internal LevelDBWalletIndexer WalletIndexer { get { return _walletIndexer; } }
        internal LevelDBProperty Properties { get { return _properties; } }
        internal CacheBlocks CacheBlocks { get { return _cacheBlocks; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public bool Initialize(Block genesisBlock)
        {
            bool result = false;

            if (_blockChain.TryGetVersion(out Version version))
            {
                if (_blockChain.TryGetCurrentBlock(out UInt256 currentHash, out uint currentHeight))
                {
                    _cacheBlocks = new CacheBlocks((uint)(currentHeight * 1.1F));

                    uint index = 0;
                    IEnumerable<UInt256> headerHashs = _blockChain.GetHeaderHashList();
                    foreach (UInt256 headerHash in headerHashs)
                    {
                        _cacheBlocks.AddHeaderHash(index++, headerHash);
                    }

                    if (index == 0)
                    {
                        foreach (BlockHeader blockHeader in _blockChain.GetBlockHeaderList())
                            _cacheBlocks.AddHeaderHash(blockHeader.Height, blockHeader.Hash);
                    }
                    else if (index <= currentHeight)
                    {
                        UInt256 hash = currentHash;
                        Dictionary<uint, UInt256> headers = new Dictionary<uint, UInt256>();

                        while (hash != _cacheBlocks.GetBlockHash((uint)_cacheBlocks.HeaderCount - 1))
                        {
                            BlockState blockState = _blockChain.Storage.Block.Get(hash);
                            if (blockState != null)
                            {
                                headers.Add(blockState.Header.Height, blockState.Header.Hash);
                                hash = blockState.Header.PrevHash;
                            }
                        }

                        foreach (var header in headers.OrderBy(x => x.Key))
                            _cacheBlocks.AddHeaderHash(header.Key, header.Value);
                    }

                    result = true;
                }
                else
                {
                    Logger.Error("[Error] " + MethodBase.GetCurrentMethod().Name + " : " + "Not found lastest block.");
                }
            }
            else
            {
                _blockChain.PutVersion(Assembly.GetExecutingAssembly().GetName().Version);

                _cacheBlocks = new CacheBlocks(_defaultCacheCapacity);
                _cacheBlocks.AddHeaderHash(genesisBlock.Height, genesisBlock.Hash);
                result = true;
            }

            return result;
        }

        public void SwitchFork(Block newBlock)
        {
            Block lastBlock = _blockChain.GetBlock(_blockChain.GetCurrentBlockHash());

            KeyValuePair<List<BlockHeader>, List<BlockHeader>> branches = _fork_db.GetBranch(newBlock.Hash, lastBlock.Hash);

            foreach (BlockHeader header in branches.Value)
            {
                _fork_db.Pop();
            }

            foreach (BlockHeader header in branches.Key)
            {
                try
                {
                    //ApplyBlock();
                }
                catch (Exception e)
                {
                    foreach (BlockHeader keyHeader in branches.Key)
                        _fork_db.Pop();

                    foreach (BlockHeader ValueHeader in branches.Value)
                        //ApplyBlock();

                    break;
                }
            }
        }
        #endregion
    }
}
