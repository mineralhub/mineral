using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public interface ISourceInter<K, V>
    {
        void PutData(K key, V value);
        void PutData(K key, V value, WriteOptionWrapper options);
        V GetData(K key);
        void DeleteData(K key);
        void DeleteData(K key, WriteOptionWrapper options);
        bool Flush();
    }
}
