using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Utils
{
    public static class ArrayUtils
    {
        public static int[] ToIntArray(this string value)
        {
            int length = value.Length;
            int[] result = new int[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = int.Parse(value[i].ToString());
            }

            return result;
        }

        public static byte[] SubArray(this byte[] input, uint start, uint end)
        {
            byte[] result = new byte[end - start];
            Array.Copy(input, start, result, 0, end - start);

            return result;
        }
    }
}
