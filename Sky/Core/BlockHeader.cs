using System.IO;
using Sky.Cryptography;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Sky.Core
{
    public class BlockHeader : IVerifiable
    {
        public UInt256 PrevHash = null;
        public UInt256 MerkleRoot = null;
        public int Version = 0;
        public int Timestamp = 0;
        public int Height = 0;
        public MakerSignature Signature = null;

        protected UInt256 _hash = null;
        public UInt256 Hash
        {
            get
            {
                if (_hash == null)
                    _hash = this.GetHash();
                return _hash;
            }
        }
        public int Size => PrevHash.Size + MerkleRoot.Size + sizeof(int) + sizeof(int) + sizeof(int) + Signature.Size;

        public BlockHeader()
        {
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

        public byte[] ToUnsignedArray()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                SerializeUnsigned(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            PrevHash = reader.ReadSerializable<UInt256>();
            MerkleRoot = reader.ReadSerializable<UInt256>();
            Version = reader.ReadInt32();
            Timestamp = reader.ReadInt32();
            Height = reader.ReadInt32();
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            writer.WriteSerializable(PrevHash);
            writer.WriteSerializable(MerkleRoot);
            writer.Write(Version);
            writer.Write(Timestamp);
            writer.Write(Height);
        }

        public void Deserialize(BinaryReader reader)
        {
            DeserializeUnsigned(reader);
            Signature = reader.ReadSerializable<MakerSignature>();
        }

        public void Serialize(BinaryWriter writer)
        {
            SerializeUnsigned(writer);
            writer.WriteSerializable(Signature);
        }

        public void Sign(ECKey key)
        {
            Signature = new MakerSignature(Cryptography.Helper.Sign(ToUnsignedArray().SHA256(), key), key.PublicKey.ToByteArray());
        }

        public bool VerifySignature()
        {
            return Cryptography.Helper.VerifySignature(Signature, ToUnsignedArray().SHA256());
        }

        public bool Verify()
        {
            if (!VerifySignature())
                return false;
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
