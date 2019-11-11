using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public interface IRevokingDB : IDatabase<byte[]>, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        void SetMode(bool mode);
        List<byte[]> GetLatestValues(long limit);
        List<byte[]> GetValuesNext(byte[] key, long limit);
        List<byte[]> GetValuesPrevious(byte[] key, long limit);
        Dictionary<byte[], byte[]> GetAllValues();
    }
}
