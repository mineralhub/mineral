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
    }
}
