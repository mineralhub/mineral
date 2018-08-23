using System.IO;
using Sky.Cryptography;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class BlockHeader : IVerifiable
    {
        protected UInt256 _prevHash = null;
        protected UInt256 _merkleRoot = null;
        protected int _version = 0;
        protected int _timestamp = 0;
        protected int _height = 0;
        protected MakerSignature _signature = null;

        public UInt256 PrevHash => _prevHash;
        public UInt256 MerkleRoot => _merkleRoot;
        public int Version => _version;
        public int Timestamp => _timestamp;
        public int Height => _height;
        public MakerSignature Signature => _signature;

        public UInt256 Hash => this.GetHash();
        public int Size => _prevHash.Size + _merkleRoot.Size + sizeof(int) + sizeof(int) + sizeof(int);

        public BlockHeader()
        {
        }

        private BlockHeader(int height, int version, int timestamp, UInt256 merkleRoot, UInt256 prevHash)
        {
            _height = height;
            _version = version;
            _timestamp = timestamp;
            _merkleRoot = merkleRoot;
            _prevHash = prevHash;
        }

        public BlockHeader(int height, int version, int timestamp, UInt256 merkleRoot, UInt256 prevHash, MakerSignature sign)
            : this(height, version, timestamp, merkleRoot, prevHash)
        {
            _signature = sign;
        }

        public BlockHeader(int height, int version, int timestamp, UInt256 merkleRoot, UInt256 prevHash, ECKey key)
            : this(height, version, timestamp, merkleRoot, prevHash)
        {
            _signature = new MakerSignature(Cryptography.Helper.Sign(Hash.Data, key), key.PublicKey.ToByteArray());
        }

        public static BlockHeader FromArray(byte[] data, int startIndex)
        {
            BlockHeader header = new BlockHeader();
            using (MemoryStream ms = new MemoryStream(data, startIndex, data.Length - startIndex, false))
            using (BinaryReader br = new BinaryReader(ms))
            {
                header.Deserialize(br);
                return header;
            }
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            _prevHash = reader.ReadSerializable<UInt256>();
            _merkleRoot = reader.ReadSerializable<UInt256>();
            _version = reader.ReadInt32();
            _timestamp = reader.ReadInt32();
            _height = reader.ReadInt32();
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            writer.WriteSerializable(_prevHash);
            writer.WriteSerializable(_merkleRoot);
            writer.Write(_version);
            writer.Write(_timestamp);
            writer.Write(_height);
        }

        public void Deserialize(BinaryReader reader)
        {
            DeserializeUnsigned(reader);
            _signature = reader.ReadSerializable<MakerSignature>();
        }

        public void Serialize(BinaryWriter writer)
        {
            SerializeUnsigned(writer);
            writer.WriteSerializable(_signature);
        }

        public bool Verify()
        {
            if (Hash == Blockchain.Instance.GenesisBlock.Hash)
                return false;
            if (Blockchain.Instance.ContainsBlock(Hash))
                return false;
            return true;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["prevhash"] = PrevHash.ToString();
            json["merkleroot"] = MerkleRoot.ToString();
            json["version"] = Version;
            json["timestamp"] = Timestamp;
            json["height"] = Height;
            json["signature"] = Signature.ToJson();
            return json;
        }
    }
}
