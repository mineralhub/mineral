using System;
using System.Collections.Generic;
using System.Text;

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
