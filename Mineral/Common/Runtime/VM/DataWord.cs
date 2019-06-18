using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Mineral.Utils;

namespace Mineral.Common.Runtime.VM
{
    public class DataWord : IComparable<DataWord>
    {
        #region Field
        public static readonly int WORD_SIZE = 32;
        public static readonly int MAX_POW = 256;
        public static readonly BigInteger _2_256 = BigInteger.Pow(2, MAX_POW);
        public static readonly BigInteger MAX_VALUE = _2_256 - BigInteger.One;

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
                throw new Exception("Data word can't exceed 32 bytes : " + bytes.ToHexString());
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
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
