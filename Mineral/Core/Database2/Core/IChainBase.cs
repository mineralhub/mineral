using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public interface IChainBase<T> : IDatabase<T>, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        string GetName();
        string GetDBName();
    }
}
