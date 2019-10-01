using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public interface Flusher
    {
        void Flush(Dictionary<WrappedByteArray, WrappedByteArray> batch);
        void Close();
        void Reset();
    }
}
