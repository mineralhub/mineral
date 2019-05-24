using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public interface IBaseDB<T, V>
    {
        long Size { get; }
        bool IsEmpty { get; }

        V Get(T key);
        void Put(T key, V value);
        void Remove(T key);
    }
}
