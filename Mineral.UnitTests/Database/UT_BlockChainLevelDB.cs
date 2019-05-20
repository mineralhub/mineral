using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core.Database.LevelDB;
using Mineral.Core2;
using Mineral.Core2.Transactions;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mineral.UnitTests.Database
{
    [TestClass]
    public class UT_BlockChainLevelDB
    {
        private LevelDBBlockChain _chainDb = null;
        private WriteOptions _write_option = WriteOptions.Default;

        private WalletAccount _account = new WalletAccount(Encoding.Default.GetBytes("0"));
        private Block _block = null;


        [TestInitialize]
        public void TestSetup()
        {
            _chainDb = new LevelDBBlockChain("./output-database");

            BlockHeader header = new BlockHeader
            {
                PrevHash = UInt256.Zero,
                MerkleRoot = UInt256.Zero,
                Version = 0,
                Timestamp = 0,
                Height = 1
            };
            header.Sign(_account.Key);
            _block = new Block(header, new List<Transaction>());
        }

        [TestCleanup]
        public void TestClean()
        {
            _chainDb.Dispose();
            _chainDb = null;

            DirectoryInfo di = new DirectoryInfo("./output-database");
            if (di.Exists)
                di.Delete(true);
        }

        [TestMethod]
        public void BatchCurrentHeader()
        {
            bool result = false;
            try
            {
                uint blockHeight = 0;
                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                _chainDb.PutCurrentHeader(batch, _block.Header);
                _chainDb.BatchWrite(_write_option, batch);

                if (_chainDb.TryGetCurrentHeader(out blockHash, out blockHeight))
                    result = (_block.Header.Hash.Equals(blockHash) && _block.Height.Equals(blockHeight));
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void BatchCurrentBlock()
        {
            bool result = false;
            try
            {
                uint blockHeight = 0;
                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                _chainDb.PutCurrentBlock(batch, _block);
                _chainDb.BatchWrite(_write_option, batch);

                if (_chainDb.TryGetCurrentBlock(out blockHash, out blockHeight))
                    result = (_block.Header.Hash.Equals(blockHash) && _block.Height.Equals(blockHeight));
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void BatchBlock()
        {
            bool result = false;
            try
            {
                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                _chainDb.PutBlock(batch, _block, 1L);
                _chainDb.BatchWrite(_write_option, batch);

                Block value = null;
                if (_chainDb.TryGetBlock(_block.Hash, out value))
                    result = _block.Header.Hash.Equals(value.Hash);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void BatchTransaction()
        {
            bool result = false;
            try
            {
                WalletAccount toAddress = new WalletAccount(Encoding.Default.GetBytes("1"));
                TransferTransaction transfer = new TransferTransaction()
                {
                    From = _account.AddressHash,
                    To = new Dictionary<UInt160, Fixed8> { { toAddress.AddressHash, Fixed8.One } }
                };

                Transaction tx = new Transaction(TransactionType.Transfer, DateTime.UtcNow.ToTimestamp(), transfer);
                tx.Sign(_account);

                _block.Transactions.Add(tx);

                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                _chainDb.PutTransaction(batch, _block, tx);
                _chainDb.BatchWrite(_write_option, batch);

                Transaction value = null;
                if (_chainDb.TryGetTransaction(tx.Hash, out value))
                    result = tx.Hash.Equals(value.Hash);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void BatchTransactionResult()
        {
            bool result = false;
            try
            {
                WalletAccount toAddress = new WalletAccount(Encoding.Default.GetBytes("1"));
                TransferTransaction transfer = new TransferTransaction()
                {
                    From = _account.AddressHash,
                    To = new Dictionary<UInt160, Fixed8> { { toAddress.AddressHash, Fixed8.One } }
                };

                Transaction tx = new Transaction(TransactionType.Transfer, DateTime.UtcNow.ToTimestamp(), transfer);
                tx.Sign(_account);
                tx.Verify();

                _block.Transactions.Add(tx);

                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                _chainDb.PutTransactionResult(batch, tx);
                _chainDb.BatchWrite(_write_option, batch);

                MINERAL_ERROR_CODES code;
                if (_chainDb.TryGetTransactionResult(tx.Hash, out code))
                    result = tx.Data.TxResult.Equals(code);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void BatchCurrentTurnTable()
        {
            bool result = false;
            try
            {
                WriteBatch batch = new WriteBatch();
                TurnTableState state = new TurnTableState();
                List<UInt160> address = new List<UInt160>();

                address.Add(_account.AddressHash);
                state.SetTurnTable(address, _block.Height);

                _chainDb.PutTurnTable(batch, state);
                _chainDb.BatchWrite(_write_option, batch);

                TurnTableState resState;
                if (_chainDb.TryGetCurrentTurnTable(out resState))
                {
                    result = state.turnTableHeight.Equals(resState.turnTableHeight);
                }
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void BatchTurnTable()
        {
            bool result = false;
            try
            {
                WriteBatch batch = new WriteBatch();
                TurnTableState state = new TurnTableState();
                List<UInt160> address = new List<UInt160>();

                address.Add(_account.AddressHash);
                state.SetTurnTable(address, _block.Height);

                _chainDb.PutTurnTable(batch, state);
                _chainDb.BatchWrite(_write_option, batch);

                TurnTableState resState;
                if (_chainDb.TryGetTurnTable(state.turnTableHeight, out resState))
                {
                    result = state.turnTableHeight.Equals(resState.turnTableHeight);
                }
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetVersion()
        {
            bool result = false;
            try
            {
                Version version = new Version("1.0");
                _chainDb.PutVersion(version);

                result = _chainDb.GetVersion().Equals(version);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetCurrentHeaderHash()
        {
            bool result = false;
            try
            {
                _chainDb.PutCurrentHeader(_block.Header);
                result = _chainDb.GetCurrentHeaderHash().Equals(_block.Header.Hash);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetCurrentHeaderHeight()
        {
            bool result = false;
            try
            {
                _chainDb.PutCurrentHeader(_block.Header);
                result = _chainDb.GetCurrentHeaderHeight().Equals(_block.Header.Height);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetCurrentBlockHash()
        {
            bool result = false;
            try
            {
                _chainDb.PutCurrentBlock(_block);
                result = _chainDb.GetCurrentBlockHash().Equals(_block.Hash);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetCurrentBlockHeight()
        {
            bool result = false;
            try
            {
                _chainDb.PutCurrentBlock(_block);
                result = _chainDb.GetCurrentBlockHeight().Equals(_block.Height);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetBlockHeader()
        {
            bool result = false;
            try
            {
                _chainDb.PutBlock(_block);
                result = _chainDb.GetBlockHeader(_block.Header.Hash).Hash.Equals(_block.Header.Hash);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetBlock()
        {
            bool result = false;
            try
            {
                _chainDb.PutBlock(_block);
                result = _chainDb.GetBlock(_block.Hash).Hash.Equals(_block.Hash);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetHeaderHashList()
        {
            bool result = false;
            try
            {
                List<UInt256> hashList = new List<UInt256>();
                for (int i = 0; i < 5; i++)
                {
                    BlockHeader header = new BlockHeader
                    {
                        PrevHash = UInt256.Zero,
                        MerkleRoot = UInt256.Zero,
                        Version = 0,
                        Timestamp = 0,
                        Height = (uint)i,
                    };
                    header.Sign(_account.Key);
                    _block = new Block(header, new List<Transaction>());
                    hashList.Add(_block.Hash);
                }
                _chainDb.PutHeaderHashList(1, hashList);
                result = new List<UInt256>(_chainDb.GetHeaderHashList()).Count.Equals(hashList.Count);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetBlockHeaders()
        {
            bool result = false;
            try
            {
                _chainDb.PutBlock(_block);
                result = new List<BlockHeader>(_chainDb.GetBlockHeaders(0, 1)).Count > 0;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }


        [TestMethod]
        public void PutGetBlockHeaderList()
        {
            bool result = false;
            try
            {
                _chainDb.PutBlock(_block);
                result = new List<BlockHeader>(_chainDb.GetBlockHeaderList()).Count > 0;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetBlockHeaderHashList()
        {
            bool result = false;
            try
            {
                _chainDb.PutBlock(_block);
                result = new List<UInt256>(_chainDb.GetBlockHeaderHashList()).Count > 0;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetCurrentTurnTable()
        {
            bool result = false;
            try
            {
                TurnTableState state = new TurnTableState();
                List<UInt160> address = new List<UInt160>();

                address.Add(_account.AddressHash);
                state.SetTurnTable(address, _block.Height);

                _chainDb.PutTurnTable(state);
                TurnTableState res = _chainDb.GetCurrentTurnTable();
                result = state.turnTableHeight.Equals(res.turnTableHeight);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetTurnTable()
        {
            bool result = false;
            try
            {
                TurnTableState state = new TurnTableState();
                List<UInt160> address = new List<UInt160>();

                address.Add(_account.AddressHash);
                state.SetTurnTable(address, _block.Height);

                _chainDb.PutTurnTable(state);
                TurnTableState res = _chainDb.GetTurnTable(_block.Height);
                result = state.turnTableHeight.Equals(res.turnTableHeight);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetTurnHeightList()
        {
            bool result = false;
            try
            {
                TurnTableState state = new TurnTableState();
                List<UInt160> address = new List<UInt160>();

                address.Add(_account.AddressHash);
                state.SetTurnTable(address, _block.Height);

                _chainDb.PutTurnTable(state);
                result = new List<uint>(_chainDb.GetTurnTableHeightList(_block.Height)).Count > 0;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void PutGetDelegateStateAll()
        {
            bool result = false;
            try
            {
                WriteBatch batch = new WriteBatch();
                byte[] name = Encoding.Default.GetBytes("delegate");
                UInt160 addressHash = _account.AddressHash;
                DelegateState state = new DelegateState(addressHash, name);

                batch.Put(SliceBuilder.Begin(DataEntryPrefix.ST_Delegate).Add(addressHash), SliceBuilder.Begin().Add(state));
                _chainDb.BatchWrite(_write_option, batch);

                List<DelegateState> delegates = new List<DelegateState>(_chainDb.GetDelegateStateAll());
                result = delegates.Find(x => x.Name.SequenceEqual(name) && x.AddressHash == addressHash) != null;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }
    }

}
