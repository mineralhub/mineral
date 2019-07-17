using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mineral.Cryptography;

namespace Mineral.Common.Stroage.LevelDB
{
    internal static class Helper
    {
        public static uint Murmur32(this IEnumerable<byte> value, uint seed)
        {
            using (Murmur3 murmur = new Murmur3(seed))
            {
                return murmur.ComputeHash(value.ToArray()).ToUInt32(0);
            }
        }

        public static byte[] ToArray(this ISerializable value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                value.Serialize(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}
