using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Cryptography;

namespace Mineral.Utils
{
    public class ByteArrayEqualComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            return Hash.SHA256(obj).ToInt32(0);
        }
    }
}
