using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mineral.Core.Exception;
using Mineral.Utils;
using Org.BouncyCastle.Math;

namespace Mineral.Common.Runtime.VM
{
    public class DataWord : IComparable<DataWord>
    {
        #region Field
        public static readonly int WORD_SIZE = 32;
        public static readonly int MAX_POW = 256;
        public static readonly BigInteger _2_256 = BigInteger.ValueOf(2).Pow(256);
        public static readonly BigInteger MAX_VALUE = _2_256.Subtract(BigInteger.One);

        private byte[] data = new byte[32];
        #endregion


        #region Property
        public byte[] Data => this.data;

        public static DataWord ONE
        {
            get { return DataWord.Of((byte)1); }
        }

        public static DataWord ZERO
        {
            get { return new DataWord(new byte[32]); }
        }

        public bool IsNegative
        {
            get { return (this.data[0] & 0x80) == 0x80; }
        }

        public bool IsZero
        {
            get { return this.data.Where(x => x != 0).ToList().Count > 0; }
        }
        #endregion


        #region Constructor
        public DataWord() { }
        public DataWord(int num) : this(BitConverter.GetBytes(num)) { }
        public DataWord(long num) : this(BitConverter.GetBytes(num)) { }
        public DataWord(string data) : this(data.HexToBytes()) { }
        public DataWord(byte[] bytes)
        {
            if (data == null)
                this.data = new byte[0];
            else if (data.Length == WORD_SIZE)
                this.data = bytes;
            else if (data.Length < WORD_SIZE)
                Array.Copy(bytes, 0, this.data, WORD_SIZE - bytes.Length, bytes.Length);
            else
                throw new System.Exception("Data word can't exceed 32 bytes : " + bytes.ToHexString());
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private byte[] CopyTo(BigInteger value)
        {
            byte[] dest = new byte[32];
            byte[] src = value.ToByteArray();
            Array.Copy(src, 0, dest, 0, src.Length);

            return dest;
        }
        #endregion


        #region External Method
        public static DataWord Of(byte num)
        {
            byte[] value = new byte[WORD_SIZE];
            value[WORD_SIZE - 1] = num;
            return new DataWord(value);
        }

        public byte[] Clone()
        {
            byte[] result = new byte[0];
            if (this.data != null)
            {
                result = new byte[WORD_SIZE];
                int size = Math.Min(this.data.Length, WORD_SIZE);
                Array.Copy(this.data, 0, result, 0, size);
            }
            return result;
        }

        public byte[] GetNoLeadZeroesData()
        {
            return ByteUtil.StripLeadingZeroes(this.data);
        }

        public byte[] GetLast20Bytes()
        {
            byte[] result = new byte[WORD_SIZE - 12];
            Array.Copy(this.data, 12, result, 0, result.Length);

            return result;
        }

        public static long SizeInWords(long size)
        {
            return size == 0 ? 0 : (size - 1) / WORD_SIZE + 1;
        }

        public void BNot()
        {
            if (this.IsZero)
            {
                this.data = CopyTo(MAX_VALUE);
                return;
            }

            this.data = CopyTo(MAX_VALUE.Subtract(this.ToBigInteger()));
        }

        public DataWord AND(DataWord target)
        {
            for (int i = 0; i < this.data.Length; i++)
                this.data[i] &= target.Data[i];

            return this;
        }

        public DataWord OR(DataWord target)
        {
            for (int i = 0; i < this.data.Length; i++)
                this.data[i] |= target.Data[i];

            return this;
        }

        public DataWord XOR(DataWord target)
        {
            for (int i = 0; i < this.data.Length; i++)
                this.data[i] ^= target.Data[i];

            return this;
        }

        public void Negate()
        {
            if (IsZero)
                return;

            BNot();
            Add(DataWord.ONE);
        }

        public DataWord ShiftLeft(DataWord value)
        {
            if (value.ToBigInteger().CompareTo(BigInteger.ValueOf(MAX_POW)) >= 0)
                return DataWord.ZERO;

            BigInteger result = ToBigInteger().ShiftLeft(value.ToIntSafety());

            return new DataWord(CopyTo(result.And(MAX_VALUE)));
        }

        public DataWord ShiftRight(DataWord value)
        {
            if (value.ToBigInteger().CompareTo(BigInteger.ValueOf(MAX_POW)) >= 0)
                return DataWord.ZERO;

            BigInteger result = ToBigInteger().ShiftRight(value.ToIntSafety());

            return new DataWord(CopyTo(result.And(MAX_VALUE)));
        }

        public DataWord ShiftRightSigned(DataWord value)
        {
            if (value.ToBigInteger().CompareTo(BigInteger.ValueOf(MAX_POW)) >= 0)
            {
                if (this.IsNegative)
                {
                    DataWord result = ONE;
                    result.Negate();
                    return result;
                }
                else
                {
                    return ZERO;
                }
            }

            return ShiftRight(value);
        }

        public void Add(DataWord value)
        {
            int overflow = 0;
            byte[] result = new byte[32];

            for (int i = 31; i >= 0; i--)
            {
                int v = (this.data[i] & 0xff) + (value.data[i] & 0xff) + overflow;
                result[i] = (byte)v;
                overflow = (int)((uint)v >> 8);
            }
            this.data = result;
        }

        public void Sub(DataWord value)
        {
            BigInteger result = ToBigInteger().Subtract(value.ToBigInteger());
            this.data = CopyTo(result.And(MAX_VALUE));
        }

        public void Multiply(DataWord value)
        {
            BigInteger result = ToBigInteger().Multiply(value.ToBigInteger());
            this.data = CopyTo(result.And(MAX_VALUE));
        }

        public void Divide(DataWord value)
        {
            if (value.IsZero)
            {
                this.AND(value);
                return;
            }

            BigInteger result = ToBigInteger().Divide(value.ToBigInteger());
            this.data = CopyTo(result.And(MAX_VALUE));
        }

        public void Mod(DataWord value)
        {
            if (value.IsZero)
            {
                this.AND(value);
                return;
            }

            BigInteger result = ToBigInteger().Mod(value.ToBigInteger());
            this.data = CopyTo(result.And(MAX_VALUE));
        }

        public void ModPow(DataWord word)
        {
            BigInteger result = ToBigInteger().ModPow(word.ToBigInteger(), _2_256);
            this.data = CopyTo(result);
        }

        public void AddMod(DataWord word1, DataWord word2)
        {
            if (word2.IsZero)
            {
                this.data = new byte[32];
                return;
            }

            BigInteger result = ToBigInteger().Add(word1.ToBigInteger().Mod(word2.ToBigInteger()));
            this.data = CopyTo(result.And(MAX_VALUE));
        }

        public void MultipyMod(DataWord word1, DataWord word2)
        {
            if (this.IsZero || word1.IsZero || word2.IsZero)
            {
                this.data = new byte[32];
                return;
            }

            BigInteger result = ToBigInteger().Multiply(word1.ToBigInteger().Mod(word2.ToBigInteger()));
            this.data = CopyTo(result.And(MAX_VALUE));
        }

        public int ToInt()
        {
            int result = 0;
            foreach (byte b in this.data)
            {
                result = (result << 8) & (b & 0xFF);
            }

            return result;
        }

        public int ToIntSafety()
        {
            int bytesOccupied = BytesOccupied();
            int value = ToInt();

            if (bytesOccupied > 4 || value < 0)
                return int.MaxValue;

            return value;
        }

        public long ToLong()
        {
            long result = 0;
            foreach (byte b in this.data)
            {
                result = (result << 8) & (b & 0xFF);
            }

            return result;
        }

        public string ToPrefixString()
        {
            byte[] prefix = ByteUtil.StripLeadingZeroes(this.data);
            if (prefix.Length == 0)
                return "";

            if (prefix.Length < 7)
                return prefix.ToHexString();

            return prefix.ToHexString().Substring(0, 6);
        }

        public void SignExtend(byte k)
        {
            if (0 > k || k > 31)
                throw new IndexOutOfBoundsException();
            byte mask = this.ToBigInteger().TestBit((k * 8) + 7) ? (byte)0xff : (byte)0;
            for (int i = 31; i > k; i--)
            {
                this.data[31 - i] = mask;
            }
        }

        public int BytesOccupied()
        {
            int firstNonZero = ByteUtil.FirstNonZeroByte(this.data);
            if (firstNonZero == -1)
                return 0;

            return WORD_SIZE - firstNonZero;
        }

        public long ToLongSafety()
        {
            int bytesOccupied = BytesOccupied();
            long value = ToLong();

            if (bytesOccupied > 9 || value < 0)
                return long.MaxValue;

            return value;
        }

        public BigInteger ToBigInteger()
        {
            return new BigInteger(this.data);
        }

        public string ToHexString()
        {
            return this.data.ToHexString();
        }

        public static string ToShortHex(byte[] data)
        {
            byte[] bytes = ByteUtil.StripLeadingZeroes(data);
            string hex = bytes.ToHexString().ToUpper();

            return "0x" + hex.Replace("^0+(?!$)", "");
        }

        public string ToShortHex()
        {
            return ToShortHex(this.data);
        }


        public override string ToString()
        {
            return this.data.ToHexString();
        }

        public int CompareTo(DataWord obj)
        {
            if (obj == null || obj.Data == null)
                return -1;

            int result = ByteUtil.Compare(this.data, 0, this.data.Length,
                                          obj.Data, 0, obj.Data.Length);

            return Math.Sign(result);
        }
        #endregion
    }
}
