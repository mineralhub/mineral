using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public interface IRevokingDB : IDatabase<byte[]>
    {
        void SetMode(bool mode);
        HashSet<byte[]> GetLastestValues(long limit);
        HashSet<byte[]> GetValuesNext(long limit);
        HashSet<byte[]> GetValuesPrevious(long limit);
        Dictionary<byte[], byte[]> GetAllValues();
    }
}
