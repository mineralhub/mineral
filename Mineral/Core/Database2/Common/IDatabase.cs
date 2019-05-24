using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public interface IDatabase<T>
    {
        void Reset();
        void Close();
        void Put(byte[] key, T item);
        void Delete(byte[] key);

        bool Contains(byte[] key);
        T Get(byte[] key);
        T GetUnchecked(byte[] key);
    }
}
