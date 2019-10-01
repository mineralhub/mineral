using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public interface IDBSourceInter<V> : IBatchSourceInter<byte[], V>, IEnumerable<KeyValuePair<byte[], V>>
    {
        string DataBaseName { get; set; }
        bool IsAlive { get; set; }

        void Init();
        void Close();
        void Reset();

        HashSet<byte[]> AllKeys();
        HashSet<byte[]> AllValue();
        long GetTotal();
    }
}
