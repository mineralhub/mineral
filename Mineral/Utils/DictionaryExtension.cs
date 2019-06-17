using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Utils
{
    public static class DictionaryExtension
    {
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
    }
}
