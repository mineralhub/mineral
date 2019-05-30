using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mineral.Utils
{
    public static class StringHelper
    {
        public static string GetString(this byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }

        public static string GetString(this int[] value, string seperator = "")
        {
            return string.Join(seperator, value.Select(x => x.ToString()).ToArray());
        }

        public static byte[] GetBytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return value.IsNullOrEmpty();
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !value.IsNullOrEmpty();
        }
    }
}
