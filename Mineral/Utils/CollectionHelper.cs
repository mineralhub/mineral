using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Utils
{
    public static class CollectionHelper
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection.IsNullOrEmpty();
        }

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection.IsNullOrEmpty();
        }

        public static List<T> Truncate<T>(this List<T> collection, int limit)
        {
            if (limit > collection.Count)
            {
                return new List<T>(collection);
            }

            List<T> result = new List<T>();
            foreach (T item in collection)
            {
                result.Add(item);
                if (result.Count == limit)
                {
                    break;
                }
            }

            return result;
        }
    }
}
