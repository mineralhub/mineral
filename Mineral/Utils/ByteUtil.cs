using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

namespace Mineral.Utils
{
    public static class ByteUtil
    {
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
