using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public interface IDatabase<T> : IDB, IEnumerable<KeyValuePair<byte[], T>>
    {
        void Put(byte[] key, T value);
        void Delete(byte[] key);

        bool Contains(byte[] key);
        T Get(byte[] key);
        T GetUnchecked(byte[] key);
    }
}
