using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mineral.Utils
{
    public static class ArrayUtil
    {
        public static T[] Copy<T>(T[] source)
        {
            if (source == null)
                throw new ArgumentException("Source is null");

            T[] result = new T[source.Length];
            Array.Copy(source, 0, result, 0, source.Length);

            return result;
        }

        public static T[] CopyRange<T>(T[] input, int start, int end)
        {
            int length = end - start;
            if (length < 0)
                throw new ArgumentException("from > to");

            T[] result = new T[length];
            Array.Copy(input, start, result, 0, length);

            return result;
        }

        public static T[] GetRange<T>(T[] input, int offset, int length)
        {
            if (offset >= input.Length || length == 0)
                return null;

            T[] result = new T[length];
            Array.Copy(input, 0, result, 0, length);

            return result;
        }

        public static bool SequenceEqual<T>(T[] source1, T[] source2)
        {
            return source1.SequenceEqual(source2);
        }

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

        public static T[] SubArray<T>(this T[] input, int start, int end)
        {
            T[] result = new T[end - start];
            Array.Copy(input, start, result, 0, end - start);

            return result;
        }

        public static bool IsEqualCollection<T>(ICollection<T> a, ICollection<T> b)
        {
            if (a.Count != b.Count)
                return false;

            Dictionary<T, int> d = new Dictionary<T, int>();
            foreach (T item in a)
            {
                int c = 0;
                if (d.TryGetValue(item, out c))
                    d[item] = c + 1;
                else
                    d.Add(item, 1);
            }

            foreach (T item in b)
            {
                int c;
                if (d.TryGetValue(item, out c))
                {
                    if (c == 0)
                        return false;
                    else
                        d[item] = c - 1;
                }
                else
                {
                    return false;
                }
            }

            foreach (int v in d.Values)
            {
                if (v != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
