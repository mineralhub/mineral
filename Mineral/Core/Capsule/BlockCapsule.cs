using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Utils;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class BlockCapsule : IProtoCapsule<Block>
    {
        public class BlockId : SHA256Hash, IComparable
        {
            private long num;

            public long Num { get { return this.num; } }

            public BlockId() : base(SHA256Hash.ZERO_HASH.Hash) { this.num = 0; }
            public BlockId(SHA256Hash hash, long num) : base(num, hash) { this.num = num; }
            public BlockId(byte[] hash, long num) : base(num, hash) { this.num = num; }
            public BlockId(ByteString hash, long num) : base(num, hash.ToByteArray()) { this.num = num; }
            public BlockId(SHA256Hash block_id)
                : base(block_id.Hash)
            {
                byte[] block_num = new byte[8];
                Array.Copy(block_id.Hash, 0, block_num, 0, 8);
                this.num = BitConverter.ToInt64(block_num, 0);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                    return true;

                if (obj == null || (GetType() != obj.GetType() && !(obj is SHA256Hash)))
                    return false;

                return this.Hash.SequenceEqual(((SHA256Hash)obj).Hash);
            }

            public int CompareTo(object obj)
            {
                if (obj.GetType().Equals(typeof(BlockId)))
                {
                    throw new ArgumentException("Compare type is not BlockId");
                }

                return base.CompareTo((BlockId)obj);
            }

            public string GetString()
            {
                return "Num : " + this.num + ", Id : " + base.ToString();
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }

        #region Field
        private BlockId block_id = new BlockId(SHA256Hash.ZERO_HASH, 0);
        private Block block = null;
        private bool generate_by_myself = false;
        private List<TransactionCapsule> transactions = new List<TransactionCapsule>();
        #endregion


        #region Property
        public BlockId Id
        {
            get
            {
                if (this.block_id.Equals(SHA256Hash.ZERO_HASH))
                    this.block_id = new BlockId(SHA256Hash.Of(this.block.BlockHeader.RawData.ToByteArray()), this.block.BlockHeader.RawData.Number);

                return this.block_id;
            }
        }

        public Block Instance => this.block;
        public byte[] Data => this.block?.ToByteArray();

        public long Num
        {
            get { return this.block.BlockHeader.RawData.Number; }
        }

        public long Timestamp
        {
            get { return this.block.BlockHeader.RawData.Timestamp; }
        }

        public BlockId ParentId
        {
            get { return new BlockId(this.block.BlockHeader.RawData.ParentHash, Num - 1); }
        }

        public SHA256Hash MerkleRoot
        {
            get { return SHA256Hash.Wrap(this.block.BlockHeader.RawData.TxTrieRoot); }
        }

        public SHA256Hash AccountRoot
        {
            get
            {
                return this.block.BlockHeader.RawData.AccountStateRoot.IsNotNullOrEmpty() ?
                    SHA256Hash.Wrap(this.block.BlockHeader.RawData.AccountStateRoot) : SHA256Hash.ZERO_HASH;
            }
        }

        public ByteString WitnessAddress
        {
            get { return this.block.BlockHeader.RawData.WitnessAddress; }
        }

        public bool IsGenerateMyself
        {
            get { return this.generate_by_myself; }
            set { this.generate_by_myself = value; }
        }

        public List<TransactionCapsule> Transactions
        {
            get { return this.transactions; }
            set { this.transactions = value; }
        }
        #endregion


        #region Constructor
        public BlockCapsule(Block block)
        {
            this.block = block;
            InitTransaction();
        }

        public BlockCapsule(long number, SHA256Hash hash, long timestamp, ByteString witness_address)
        {
            BlockHeader header = new BlockHeader() { RawData = new BlockHeader.Types.raw() };
            header.RawData.Number = number;
            header.RawData.ParentHash = hash.Hash.ToByteString();
            header.RawData.Timestamp = timestamp;
            header.RawData.Version = Parameter.ChainParameters.BLOCK_VERSION;
            header.RawData.WitnessAddress = witness_address;

            this.block = new Block();
            this.block.BlockHeader = header;

            InitTransaction();
        }

        public BlockCapsule(long timestamp, ByteString parent_hash, long number, List<Transaction> transations)
        {
            BlockHeader header = new BlockHeader() { RawData = new BlockHeader.Types.raw() };
            header.RawData.Timestamp = timestamp;
            header.RawData.ParentHash = parent_hash;
            header.RawData.Number = number;

            this.block = new Block();
            transations.ForEach(tx => block.Transactions.Add(tx));
            this.block.BlockHeader = header;
            InitTransaction();
        }

        public BlockCapsule(byte[] data)
        {
            try
            {
                this.block = Block.Parser.ParseFrom(data);
                InitTransaction();
            }
            catch (System.Exception e)
            {
                throw new ArgumentException("Block proto data parse exception", e);
            }
        }

        public BlockCapsule(CodedInputStream stream)
        {
            try
            {
                this.block = Block.Parser.ParseFrom(stream);
                InitTransaction();
            }
            catch (System.Exception e)
            {
                Logger.Error("Contractor block error : " + e.Message);
                throw new ArgumentException("Block proto data parse exception");
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void InitTransaction()
        {
            this.transactions = this.block.Transactions.Select(tx => new TransactionCapsule(tx)).ToList();
        }
        #endregion


        #region External Method
        public static BlockCapsule GenerateGenesisBlock()
        {
            List<Transaction> transactions = Args.Instance.GenesisBlock.Assets.Select(asset =>
            {
                return TransactionCapsule.GenerateGenesisTransaction(Wallet.Base58ToAddress(asset.Address), asset.Balance);
            }).ToList();

            BlockCapsule result = new BlockCapsule((int)Args.Instance.GenesisBlock.Timestamp,
                                                   ByteString.CopyFrom(Args.Instance.GenesisBlock.ParentHash),
                                                   Args.Instance.GenesisBlock.Number,
                                                   transactions);

            result.SetMerkleTree();
            result.SetWitness("A new system must allow existing systems to be linked together without "
                              + "requiring any central control or coordination");
            result.IsGenerateMyself = true;

            return result;
        }

        public void AddTransaction(TransactionCapsule tx)
        {
            this.block.Transactions.Add(tx.Instance);
            this.transactions.Add(tx);
        }

        public void Sign(byte[] privatekey)
        {
            ECKey ec_key = ECKey.FromPrivateKey(privatekey);
            ECDSASignature signature = ec_key.Sign(this.GetRawHash().Hash);

            this.block.BlockHeader.WitnessSignature = ByteString.CopyFrom(signature.ToByteArray());
        }

        public bool ValidateSignature(DatabaseManager db_manager)
        {
            try
            {
                ECDSASignature signature = ECDSASignature.ExtractECDSASignature(this.block.BlockHeader.WitnessSignature.ToByteArray());

                byte[] signature_address = ECKey.SignatureToAddress(GetRawHash().Hash, signature);
                byte[] witness_address = this.block.BlockHeader.RawData.WitnessAddress.ToByteArray();

                if (db_manager.DynamicProperties.GetAllowMultiSign() != 1)
                {
                    return signature_address.SequenceEqual(witness_address);
                }
                else
                {
                    byte[] witness_permission_address = db_manager.Account.Get(witness_address)?.GetWitnessPermissionAddress();
                    return signature_address.SequenceEqual(witness_permission_address);
                }
            }
            catch (System.Exception e)
            {
                throw new ValidateSignatureException(e.Message);
            }
        }

        public SHA256Hash CalcMerkleRoot()
        {
            if (this.block.Transactions.IsNullOrEmpty())
            {
                return SHA256Hash.ZERO_HASH;
            }

            return new SHA256Hash(
                new MerkleTree(
                    this.block.Transactions
                                .Select(tx => SHA256Hash.Of(tx.ToByteArray())).ToList()
                                .Select(hash => hash.Hash).ToList()
                    ).RootHash.ToArray());
        }

        public void SetMerkleTree()
        {
            this.block.BlockHeader.RawData.TxTrieRoot = ByteString.CopyFrom(CalcMerkleRoot().Hash);
        }

        public void SetAccountStateRoot(byte[] root)
        {
            this.block.BlockHeader.RawData.AccountStateRoot = ByteString.CopyFrom(root);
        }

        public void SetWitness(string witness)
        {
            this.block.BlockHeader.RawData.WitnessAddress = ByteString.CopyFrom(witness.ToBytes());
        }

        public SHA256Hash GetRawHash()
        {
            return SHA256Hash.Of(this.block.BlockHeader.RawData.ToByteArray());
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("BlockCapsule \n[ ");
            builder.Append("hash=").Append(Id).Append("\n");
            builder.Append("number=").Append(Num).Append("\n");
            builder.Append("parentId=").Append(ParentId.ToString()).Append("\n");
            builder.Append("witness address=").Append(WitnessAddress.ToByteArray().ToHexString()).Append("\n");

            builder.Append("generated by myself=").Append(generate_by_myself).Append("\n");
            builder.Append("generate time=").Append(Timestamp.ToDateTime().ToLocalTime().ToString(@"dd\\:hh\\:mm")).Append("\n");

            if (transactions.Count > 0)
            {
                builder.Append("merkle root=").Append(MerkleRoot).Append("\n");
                builder.Append("account root=").Append(AccountRoot).Append("\n");
                builder.Append("txs size=").Append(this.transactions.Count).Append("\n");
            }
            else
            {
                builder.Append("txs are empty\n");
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
    #endregion
}
