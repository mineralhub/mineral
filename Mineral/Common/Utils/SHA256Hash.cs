using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Cryptography;

namespace Mineral.Common.Utils
{
    public class SHA256Hash : IComparable<SHA256Hash>
    {
        #region Field
        public static readonly int LENGTH = 32;
        public static readonly SHA256Hash ZERO_HASH = Wrap(new byte[LENGTH]);

        private readonly byte[] hash;
        #endregion


        #region Property
        public byte[] Hash { get { return this.hash; } }
        #endregion


        #region Constructor
        public SHA256Hash(long num, SHA256Hash hash)
        {
            byte[] raw_hash = GenerateBlockId(num, hash);
            
            if (raw_hash.Length != LENGTH)
                throw new ArgumentException();

            this.hash = raw_hash;
        }

        public SHA256Hash(long num, byte[] hash)
        {
            byte[] raw_hash = GenerateBlockId(num, hash);

            if (raw_hash.Length != LENGTH)
                throw new ArgumentException();

            this.hash = raw_hash;
        }

        public SHA256Hash(byte[] raw_hash)
        {
            if (raw_hash.Length != LENGTH)
                throw new ArgumentException();

            this.hash = raw_hash;
        }

        public SHA256Hash(SHA256Hash raw_hash)
        {
            this.hash = raw_hash.Hash;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private byte[] GenerateBlockId(long block_num, SHA256Hash block_hash)
        {
            byte[] num_bytes = BitConverter.GetBytes(block_num);
            byte[] hash = new byte[block_hash.Hash.Length];
            Array.Copy(num_bytes, 0, hash, 0, 8);
            Array.Copy(block_hash.Hash, 8, hash, 8, block_hash.Hash.Length - 8);
            return hash;
        }

        private byte[] GenerateBlockId(long block_num, byte[] block_hash)
        {
            byte[] num_bytes = BitConverter.GetBytes(block_num);
            byte[] hash = new byte[block_hash.Length];
            Array.Copy(num_bytes, 0, hash, 0, 8);
            Array.Copy(block_hash, 8, hash, 8, block_hash.Length - 8);
            return hash;
        }
        #endregion


        #region External Method
        public static SHA256Hash Wrap(byte[] raw_hash_bytes)
        {
            return new SHA256Hash(raw_hash_bytes);
        }

        public static SHA256Hash Wrap(ByteString raw_hash_string)
        {
            return Wrap(raw_hash_string.ToByteArray());
        }

        public static SHA256Hash Create(byte[] contents)
        {
            return Of(contents);
        }

        public static SHA256Hash Of(byte[] contents)
        {
            return Wrap(contents.SHA256());
        }

        public static byte[] ToHash(byte[] input)
        {
            return ToHash(input, 0, input.Length);
        }

        public static byte[] ToHash(byte[] input, int offset, int length)
        {
            return input.SHA256(offset, length);
        }

        public override string ToString()
        {
            return this.hash.ToHexString();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || !obj.GetType().Equals(GetType()))
                return false;

            return this.hash.SequenceEqual(((SHA256Hash)obj).hash);
        }

        public override int GetHashCode()
        {
            return this.hash[LENGTH - 4] << 24 |
                   (this.hash[LENGTH - 3] & 0xFF) << 16 |
                   (this.hash[LENGTH - 2] & 0xFF) << 8 |
                   (this.hash[LENGTH - 1] & 0xFF);
        }

        public int CompareTo(SHA256Hash other)
        {
            for (int i = LENGTH - 1; i >= 0; i--)
            {
                int my_byte = this.hash[i] & 0xFF;
                int other_byte = other.Hash[i] & 0xFF;

                if (my_byte > other_byte)
                    return 1;
                if (my_byte < other_byte)
                    return -1;
            }
            return 0;
        }
        #endregion
    }
}
