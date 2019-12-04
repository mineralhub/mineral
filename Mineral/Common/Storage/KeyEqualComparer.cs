using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Cryptography;

namespace Mineral.Common.Storage
{
    public class KeyEqualComparer : IEqualityComparer<Key>
    {
        public bool Equals(Key x, Key y)
        {
            return x.Data.SequenceEqual(y.Data);
        }

        public int GetHashCode(Key obj)
        {
            return Hash.SHA256(obj.Data).ToInt32(0);
        }
    }
}
