using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core;
using Mineral.Core.Transactions;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mineral.UnitTests.Database
{
    [TestClass]
    public class UT_BlockChainLevelDB
    {
        LevelDBBlockChain chainDb = null;
        WriteOptions write_option = WriteOptions.Default;

        WalletAccount account = new WalletAccount(Encoding.Default.GetBytes("0"));
        Block block = null;


        [TestInitialize]
        public void TestSetup()
        {
            this.chainDb = new LevelDBBlockChain("./output-database");

            BlockHeader header = new BlockHeader
            {
                PrevHash = UInt256.Zero,
                MerkleRoot = UInt256.Zero,
                Version = 0,
                Timestamp = 0,
                Height = 10
            };
            header.Sign(this.account.Key);
            this.block = new Block(header, new List<Transaction>());
        }

        [TestCleanup]
        public void TestClean()
        {
            this.chainDb.Dispose();
        }

        [TestMethod]
        public void BatchCurrentHeader()
        {
            bool result = false;
            try
            {
                int blockHeight = 0;
                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                this.chainDb.PutCurrentHeader(batch, block.Header);
                this.chainDb.BatchWrite(this.write_option, batch);

                if (this.chainDb.TryGetCurrentHeader(out blockHash, out blockHeight))
                    result = (this.block.Header.Hash.Equals(blockHash) && this.block.Height.Equals(blockHeight));
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
                int blockHeight = 0;
                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                this.chainDb.PutCurrentBlock(batch, block);
                this.chainDb.BatchWrite(this.write_option, batch);

                if (this.chainDb.TryGetCurrentBlock(out blockHash, out blockHeight))
                    result = (block.Header.Hash.Equals(blockHash) && block.Height.Equals(blockHeight));
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

                this.chainDb.PutBlock(batch, block, 0L);
                this.chainDb.BatchWrite(this.write_option, batch);

                Block value = null;
                if (this.chainDb.TryGetBlock(this.block.Hash, out value))
                    result = block.Header.Hash.Equals(value.Hash);
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void BatchBlockFromHeader()
        {
            bool result = false;
            try
            {
                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                this.chainDb.PutBlock(batch, block.Header, 0L);
                this.chainDb.BatchWrite(this.write_option, batch);

                BlockHeader value = null;
                if (this.chainDb.TryGetBlockHeader(this.block.Hash, out value))
                    result = block.Header.Hash.Equals(value.Hash);
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
                    From = this.account.AddressHash,
                    To = new Dictionary<UInt160, Fixed8> { { toAddress.AddressHash, Fixed8.One } }
                };

                Transaction tx = new Transaction(TransactionType.Transfer, DateTime.UtcNow.ToTimestamp(), transfer);
                tx.Sign(this.account);

                block.Transactions.Add(tx);

                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                this.chainDb.PutTransaction(batch, block, tx);
                this.chainDb.BatchWrite(this.write_option, batch);

                Transaction value = null;
                if (this.chainDb.TryGetTransaction(tx.Hash, out value))
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
                    From = this.account.AddressHash,
                    To = new Dictionary<UInt160, Fixed8> { { toAddress.AddressHash, Fixed8.One } }
                };

                Transaction tx = new Transaction(TransactionType.Transfer, DateTime.UtcNow.ToTimestamp(), transfer);
                tx.Sign(this.account);
                tx.Verify();

                block.Transactions.Add(tx);

                UInt256 blockHash = new UInt256();
                WriteBatch batch = new WriteBatch();

                this.chainDb.PutTransactionResult(batch, tx);
                this.chainDb.BatchWrite(this.write_option, batch);

                MINERAL_ERROR_CODES code;
                if (this.chainDb.TryGetTransactionResult(tx.Hash, out code))
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

                address.Add(account.AddressHash);
                state.SetTurnTable(address, this.block.Height);

                this.chainDb.PutTurnTable(batch, state);
                this.chainDb.BatchWrite(this.write_option, batch);

                TurnTableState resState;
                if (this.chainDb.TryGetCurrentTurnTable(out resState))
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

                address.Add(account.AddressHash);
                state.SetTurnTable(address, this.block.Height);

                this.chainDb.PutTurnTable(batch, state);
                this.chainDb.BatchWrite(this.write_option, batch);

                TurnTableState resState;
                if (this.chainDb.TryGetTurnTable(state.turnTableHeight, out resState))
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
                this.chainDb.PutVersion(version);

                result = this.chainDb.GetVersion().Equals(version);
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
                this.chainDb.PutCurrentHeader(this.block.Header);
                result = this.chainDb.GetCurrentHeaderHash().Equals(this.block.Header.Hash);
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
                this.chainDb.PutCurrentHeader(this.block.Header);
                result = this.chainDb.GetCurrentHeaderHeight().Equals(this.block.Header.Height);
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
                this.chainDb.PutCurrentBlock(this.block);
                result = this.chainDb.GetCurrentBlockHash().Equals(this.block.Hash);
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
                this.chainDb.PutCurrentBlock(this.block);
                result = this.chainDb.GetCurrentBlockHeight().Equals(this.block.Height);
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
                this.chainDb.PutBlock(this.block);
                result = this.chainDb.GetBlockHeader(this.block.Header.Hash).Hash.Equals(this.block.Header.Hash);
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
                this.chainDb.PutBlock(this.block);
                result = this.chainDb.GetBlock(this.block.Hash).Hash.Equals(this.block.Hash);
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
                        Height = i,
                    };
                    header.Sign(this.account.Key);
                    this.block = new Block(header, new List<Transaction>());
                    hashList.Add(block.Hash);
                }
                this.chainDb.PutHeaderHashList(1, hashList);
                result = new List<UInt256>(this.chainDb.GetHeaderHashList()).Count.Equals(hashList.Count);
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
                this.chainDb.PutBlock(block);
                result = new List<BlockHeader>(this.chainDb.GetBlockHeaderList()).Count > 0;
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
                this.chainDb.PutBlock(block);
                result = new List<UInt256>(this.chainDb.GetBlockHeaderHashList()).Count > 0;
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

                address.Add(account.AddressHash);
                state.SetTurnTable(address, this.block.Height);

                this.chainDb.PutTurnTable(state);
                TurnTableState res = this.chainDb.GetCurrentTurnTable();
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

                address.Add(account.AddressHash);
                state.SetTurnTable(address, this.block.Height);

                this.chainDb.PutTurnTable(state);
                TurnTableState res = this.chainDb.GetTurnTable(this.block.Height);
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

                address.Add(account.AddressHash);
                state.SetTurnTable(address, this.block.Height);

                this.chainDb.PutTurnTable(state);
                result = new List<int>(this.chainDb.GetTurnTableHeightList(this.block.Height)).Count > 0;
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
                UInt160 addressHash = account.AddressHash;
                DelegateState state = new DelegateState(addressHash, name);

                batch.Put(SliceBuilder.Begin(DataEntryPrefix.ST_Delegate).Add(addressHash), SliceBuilder.Begin().Add(state));
                this.chainDb.BatchWrite(write_option, batch);

                List<DelegateState> delegates = new List<DelegateState>(this.chainDb.GetDelegateStateAll());
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
