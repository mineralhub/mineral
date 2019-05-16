using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core;
using Mineral.Core.Transactions;
using Mineral.Database;
using Mineral.Utils;
using Mineral.Wallets;

namespace Mineral.UnitTests.Database
{
    [TestClass]
    public class UT_ForkDatabase
    {
        private List<Block> _blocks1 = new List<Block>();
        private List<Block> _blocks2 = new List<Block>();
        private WalletAccount _account = new WalletAccount(Encoding.Default.GetBytes("0"));

        private Block lastBlock = null;
        private ForkDatabase _fork_db = new ForkDatabase();

        [TestInitialize]
        public void TestSetup()
        {
            Block block = null;
            for (uint height = 0; height < 5; height++)
            {
                block = GenerateBlock(lastBlock.Hash, height);
                _fork_db.Push(block);
                lastBlock = block;
            }

            for (uint height = 5; height < 6; height++)
                _blocks2.Add(GenerateBlock(UInt256.Zero, height));
        }

        public Block GenerateBlock(UInt256 prevHash, uint height)
        {
            BlockHeader header = new BlockHeader
            {
                PrevHash = prevHash,
                MerkleRoot = UInt256.Zero,
                Version = 0,
                Timestamp = 0,
                Height = height
            };
            header.Sign(_account.Key);
            return new Block(header, new List<Transaction>());
        }

        public void ApplyBlock(Block block)
        {
        }

        [TestMethod]
        public void SwitchFork()
        {
            foreach (Block newBlock in _blocks2)
            {
                _fork_db.Push(newBlock);
                if (!object.Equals(newBlock.Header.PrevHash, lastBlock.Header.PrevHash))
                {
                    KeyValuePair<List<Block>, List<Block>> branches = _fork_db.GetBranch(newBlock.Hash, lastBlock.Hash);

                    foreach (Block block in branches.Value)
                    {
                        _fork_db.Pop();
                    }

                    foreach (Block block in branches.Key)
                    {
                        try
                        {
                            ApplyBlock(block);
                        }
                        catch (Exception e)
                        {
                            foreach (Block key in branches.Key)
                                _fork_db.Pop();
                            foreach (Block value in branches.Value)
                                ApplyBlock(value);
                            break;
                        }
                    }
                }
            }
        }
    }
}
