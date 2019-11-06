using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Cryptography;

namespace Mineral.Core.Database2.Common
{
    public class WrapperdByteArrayEqualComparer : IEqualityComparer<WrappedByteArray>
    {
        public bool Equals(WrappedByteArray x, WrappedByteArray y)
        {
            return x.Data.SequenceEqual(y.Data);
        }

        public int GetHashCode(WrappedByteArray obj)
        {
            return Hash.SHA256(obj.Data).ToInt32(0);
        }
    }
}
