using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Google.Protobuf;

namespace Mineral.Utils
{
    public static class ByteUtil
    {
        public static byte[] Copy(byte[] source)
        {
            if (source == null)
                throw new ArgumentException("Source is null");

            byte[] result = new byte[source.Length];
            Array.Copy(source, 0, result, 0, source.Length);

            return result;
        }

        public static byte[] CopyRange(byte[] input, int start, int end)
        {
            int length = end - start;
            if (length < 0)
                throw new ArgumentException("from > to");

            byte[] result = new byte[length];
            Array.Copy(input, start, result, 0, length);

            return result;
        }

        public static byte[] GetRange(byte[] input, int offset, int length)
        {
            if (offset >= input.Length || length == 0)
                return null;

            byte[] result = new byte[length];
            Array.Copy(input, 0, result, 0, length);

            return result;
        }

        public static int Compare(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1 == null || bytes2 == null || bytes1.Length != bytes2.Length)
                throw new ArgumentNullException();

            for (int i = 0; i < bytes1.Length; i++)
            {
                int ret = ToInt(bytes1[i]) - ToInt(bytes2[i]);
                if (ret != 0)
                {
                    return ret;
                }
            }

            return 0;
        }

        public static int Compare(byte[] buffer1, int offset1, int length1, byte[] buffer2, int offset2, int length2)
        {
            if (buffer1 == buffer2 &&
                offset1 == offset2 &&
                length1 == length2)
            {
                return 0;
            }

            int end1 = offset1 + length1;
            int end2 = offset2 + length2;

            for (int i = offset1, j = offset2; i < end1 && j < end2; i++, j++)
            {
                int a = (buffer1[i] & 0xff);
                int b = (buffer2[j] & 0xff);
                if (a != b)
                {
                    return a - b;
                }
            }

            return length1 - length2;
        }

        public static int ToInt(byte value)
        {
            return value;
        }

        public static ByteString ToByteString(this byte[] bytes)
        {
            return ByteString.CopyFrom(bytes);
        }

        public static int FirstNonZeroByte(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public static byte[] StripLeadingZeroes(byte[] data)
        {
            byte[] result = null;

            if (data == null)
                return result;

            int first_non_zero = FirstNonZeroByte(data);
            switch (first_non_zero)
            {
                case -1:
                    {
                        result = new byte[0];
                    } break;
                case 0:
                    {
                        result = data;
                    } break;
                default:
                    {
                        result = new byte[data.Length - first_non_zero];
                        Array.Copy(data, first_non_zero, result, 0, data.Length - first_non_zero);
                    } break;
            }

            return result;
        }

        private static int NumberOfLeadingZeros(int i)
        {
            if (i == 0)
                return 32;
            int n = 1;
            if ((int)((uint)i >> 16) == 0) { n += 16; i <<= 16; }
            if ((int)((uint)i >> 24) == 0) { n += 8; i <<= 8; }
            if ((int)((uint)i >> 28) == 0) { n += 4; i <<= 4; }
            if ((int)((uint)i >> 30) == 0) { n += 2; i <<= 2; }
            n -= (int)((uint)i >> 31);

            return n;
        }

        public static int NumberOfLeadingZeros(byte[] bytes)
        {
            int result = 0;
            int i = FirstNonZeroByte(bytes);

            if (i == -1)
            {
                result = bytes.Length * 8;
            }
            else
            {
                int byteLeadingZeros = NumberOfLeadingZeros((int)bytes[i] & 0xff) - 24;
                result = i * 8 + byteLeadingZeros;
            }

            return result;
        }

        public static byte[] ParseBytes(byte[] input, int offset, int len)
        {
            if (offset >= input.Length || len == 0)
            {
                return new byte[0];
            }

            byte[] bytes = new byte[len];
            Array.Copy(input, offset, bytes, 0, Math.Min(input.Length - offset, len));

            return bytes;
        }

        public static byte[] ParseWord(byte[] input, int idx)
        {
            return ParseBytes(input, 32 * idx, 32);
        }

        public static byte[] ParseWord(byte[] input, int offset, int idx)
        {
            return ParseBytes(input, offset + 32 * idx, 32);
        }

        public static BigInteger BytesToBigInteger(byte[] value)
        {
            return (value == null || value.Length == 0) ? BigInteger.Zero : new BigInteger(value);
        }
    }

    public class UnsignedByteArrayCompare : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            int result;
            for (int index = 0; index < x.Length; index++)
            {
                result = ToInt(x[index]).CompareTo(ToInt(y[index]));
                if (result != 0)
                {
                    return result;
                }
            }
            return x.Length.CompareTo(y.Length);
        }

        public static int ToInt(byte value)
        {
            return value & 0xFF;
        }
    }

    public class ByteArrayCompare : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            int result;
            for (int index = 0; index < x.Length; index++)
            {
                result = ToInt(x[index]).CompareTo(ToInt(y[index]));
                if (result != 0)
                {
                    return result;
                }
            }
            return x.Length.CompareTo(y.Length);
        }

        public static int ToInt(byte value)
        {
            return value;
        }
    }
}
