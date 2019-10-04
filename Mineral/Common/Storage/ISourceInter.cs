using System;
using System.Collections.Generic;
using System.Text;
using LevelDB;

namespace Mineral.Common.Storage
{
    public interface ISourceInter<K, V>
    {
        void PutData(K key, V value);
        void PutData(K key, V value, WriteOptions options);
        V GetData(K key);
        void DeleteData(K key);
        void DeleteData(K key, WriteOptions options);
        bool Flush();
    }
}
