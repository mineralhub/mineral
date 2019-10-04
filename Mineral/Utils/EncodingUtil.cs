using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace Mineral.Utils
{
    public static class EncodingUtil
    {
        public static string BytesToString(this byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }

        public static string IntToString(this int[] value, string seperator = "")
        {
            return string.Join(seperator, value.Select(x => x.ToString()).ToArray());
        }

        public static int ToInt(byte value)
        {
            return value;
        }

        public static byte[] ToBytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static ByteString ToByteString(this byte[] bytes)
        {
            return ByteString.CopyFrom(bytes);
        }
    }
}
