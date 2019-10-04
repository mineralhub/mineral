using Mineral.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mineral.Core.Database2.Common
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
