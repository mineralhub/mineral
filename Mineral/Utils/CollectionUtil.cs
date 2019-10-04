using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

namespace Mineral.Utils
{
    public static class CollectionUtil
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return value == null || value.Length == 0;
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return value != null && value.Length > 0;
        }

        public static bool IsNullOrEmpty(this ByteString str)
        {
            return str == null || str.Length == 0;
        }

        public static bool IsNotNullOrEmpty(this ByteString str)
        {
            return str != null && str.Length > 0;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static bool IsNotNullOrEmpty<T>(this ICollection<T> list)
        {
            return list != null && list.Count > 0;
        }

        public static void Put<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static void Put<Key, Value>(this ConcurrentDictionary<Key, Value> dictionary, Key key, Value value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.TryAdd(key, value);
        }

        public static void PutAll<Key, Value>(this Dictionary<Key, Value> dictionary, Dictionary<Key, Value> other)
        {
            foreach (var item in other)
            {
                dictionary.Put(item.Key, item.Value);
            }
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

        public static bool Contains<T>(this ConcurrentQueue<T> queue, T value)
        {
            bool result = false;

            foreach (T t in queue)
            {
                if (t.Equals(value))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static bool Contains<T>(this ConcurrentBag<T> bag, T value)
        {
            bool result = false;

            foreach (T t in bag)
            {
                if (t.Equals(value))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static void Clear<T>(this BlockingCollection<T> collection)
        {
            while (collection.TryTake(out _)) ;
        }

        public static void Remove<T>(this BlockingCollection<T> collection, T item)
        {
            BlockingCollection<T> temp = new BlockingCollection<T>();

            foreach (T value in collection)
            {
                if (!value.Equals(item))
                {
                    temp.Add(value);
                }
            }

            collection.Clear();
            foreach (T value in temp)
            {
                collection.Add(value);
            }
        }

        public static void Remove<T>(this ConcurrentQueue<T> collection, T item)
        {
            ConcurrentQueue<T> temp = new ConcurrentQueue<T>();
            
            foreach (T value in collection)
            {
                if (!value.Equals(item))
                {
                    temp.Enqueue(value);
                }
            }

            collection = new ConcurrentQueue<T>(temp);
        }
    }
}
