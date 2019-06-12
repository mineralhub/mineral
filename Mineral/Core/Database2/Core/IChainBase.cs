using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public interface IMineralChainBase<T> : IDatabase<T>, IEnumerable<KeyValuePair<byte[], T>>
    {
        string GetName();
        string GetDBName();
    }
}
