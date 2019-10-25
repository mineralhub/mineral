using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Tire
{
    public interface ITrie<T>
    {
        byte[] GetRootHash();
        void SetRoot(byte[] root);
        void Clear();

        void Put(byte[] key, T value);
        T Get(byte[] key);
        void Delete(byte[] key);
        bool Flush();
    }
}
