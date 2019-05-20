using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Utils
{
    public static class StringHelper
    {
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
