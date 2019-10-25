using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Tire
{
    public class TrieKey
    {
        #region Field
        public static readonly int ODD_OFFSET_FLAG = 0x1;
        public static readonly int TERMINATOR_FLAG = 0x2;
        private readonly byte[] key;
        private readonly int off;
        private readonly bool terminal;
        #endregion


        #region Property
        public bool IsTerminal
        {
            get { return terminal; }
        }

        public bool IsEmpty
        {
            get { return GetLength() == 0; }
        }
        #endregion


        #region Contructor
        private TrieKey(byte[] key) : this(key, 0, true) { }
        public TrieKey(byte[] key, int off, bool terminal)
        {
            this.terminal = terminal;
            this.off = off;
            this.key = key;
        }

        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void SetHex(int idx, int hex)
        {
            int byteIdx = (this.off + idx) >> 1;
            if (((this.off + idx) & 1) == 0)
            {
                this.key[byteIdx] &= 0x0F;
                this.key[byteIdx] |= (byte)(hex << 4);
            }
            else
            {
                this.key[byteIdx] &= 0xF0;
                this.key[byteIdx] |= (byte)hex;
            }
        }
        #endregion


        #region External Method
        public static TrieKey FromNormal(byte[] key)
        {
            return new TrieKey(key);
        }

        public static TrieKey FromPacked(byte[] key)
        {
            return new TrieKey(key, ((key[0] >> 4) & ODD_OFFSET_FLAG) != 0 ? 1 : 2, ((key[0] >> 4) & TERMINATOR_FLAG) != 0);
        }

        public static TrieKey Empty(bool terminal)
        {
            return new TrieKey(new byte[0], 0, terminal);
        }

        public static TrieKey SingleHex(int hex)
        {
            TrieKey ret = new TrieKey(new byte[1], 1, false);
            ret.SetHex(0, hex);
            return ret;
        }

        public TrieKey Shift(int hex_count)
        {
            return new TrieKey(this.key, this.off + hex_count, terminal);
        }

        public TrieKey GetCommonPrefix(TrieKey k)
        {
            int prefix_length = 0;
            int thisLength = GetLength();
            int klen = k.GetLength();
            while (prefix_length < thisLength && prefix_length < klen && GetHex(prefix_length) == k.GetHex(prefix_length))
            {
                prefix_length++;
            }
            byte[] prefix_key = new byte[(prefix_length + 1) >> 1];
            TrieKey ret = new TrieKey(prefix_key, (prefix_length & 1) == 0 ? 0 : 1,
                prefix_length == GetLength() && prefix_length == k.GetLength() && terminal && k.IsTerminal);
            for (int i = 0; i < prefix_length; i++)
            {
                ret.SetHex(i, k.GetHex(i));
            }
            return ret;
        }

        public int GetLength()
        {
            return (this.key.Length << 1) - this.off;
        }

        public int GetHex(int idx)
        {
            byte b = this.key[(this.off + idx) >> 1];
            return (((this.off + idx) & 1) == 0 ? (b >> 4) : b) & 0xF;
        }

        public TrieKey MatchAndShift(TrieKey k)
        {
            int len = GetLength();
            int klen = k.GetLength();
            if (len < klen)
            {
                return null;
            }

            if ((this.off & 1) == (k.off & 1))
            {
                // optimization to compare whole keys bytes
                if ((this.off & 1) == 1 && GetHex(0) != k.GetHex(0))
                {
                    return null;
                }
                int idx1 = (this.off + 1) >> 1;
                int idx2 = (k.off + 1) >> 1;
                int l = klen >> 1;
                for (int i = 0; i < l; i++, idx1++, idx2++)
                {
                    if (this.key[idx1] != k.key[idx2])
                    {
                        return null;
                    }
                }
            }
            else
            {
                for (int i = 0; i < klen; i++)
                {
                    if (GetHex(i) != k.GetHex(i))
                    {
                        return null;
                    }
                }
            }
            return Shift(klen);
        }

        public TrieKey Concat(TrieKey k)
        {
            if (IsTerminal)
            {
                throw new System.Exception("Can' append to terminal key: " + this + " + " + k);
            }
            int len = GetLength();
            int klen = k.GetLength();
            int new_len = len + klen;
            byte[] newKeyBytes = new byte[(new_len + 1) >> 1];
            TrieKey ret = new TrieKey(newKeyBytes, new_len & 1, k.IsTerminal);
            for (int i = 0; i < len; i++)
            {
                ret.SetHex(i, GetHex(i));
            }
            for (int i = 0; i < klen; i++)
            {
                ret.SetHex(len + i, k.GetHex(i));
            }
            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            TrieKey k = (TrieKey)obj;
            int len = GetLength();

            if (len != k.GetLength())
            {
                return false;
            }
            for (int i = 0; i < len; i++)
            {
                if (GetHex(i) != k.GetHex(i))
                {
                    return false;
                }
            }
            return IsTerminal == k.IsTerminal;
        }

        public byte[] ToPacked()
        {
            int flags = ((this.off & 1) != 0 ? ODD_OFFSET_FLAG : 0) | (terminal ? TERMINATOR_FLAG : 0);
            byte[] ret = new byte[GetLength() / 2 + 1];
            int to = (flags & ODD_OFFSET_FLAG) != 0 ? ret.Length : ret.Length - 1;
            Array.Copy(this.key, key.Length - to, ret, ret.Length - to, to);
            ret[0] &= 0x0F;
            ret[0] |= (byte)(flags << 4);
            return ret;
        }

        public byte[] ToNormal()
        {
            if ((this.off & 1) != 0)
            {
                throw new System.Exception(
                    "Can't convert a key with odd number of hexes to normal: " + this);
            }
            int len = this.key.Length - this.off / 2;
            byte[] ret = new byte[len];
            Array.Copy(this.key, this.key.Length - len, ret, 0, len);
            return ret;
        }

        public override String ToString()
        {
            return this.key.ToHexString().Substring(this.off) + (IsTerminal ? "T" : "");
        }
        #endregion
    }
}
