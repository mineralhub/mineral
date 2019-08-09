using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Common
{
    internal interface IValueParser
    {
        void Parse(CacheBuilderSpec spec, string key, string value);
    }
}
