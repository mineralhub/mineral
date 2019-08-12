using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public interface IWeigher<TKey, TValue>
    {
        int Weigh(TKey key, TValue value);
    }

    public class OneWeigher<TKey, TValue> : IWeigher<TKey, TValue>
    {
        private static OneWeigher<TKey, TValue> instance = null;

        public static OneWeigher<TKey, TValue> Instance
        {
            get { return instance ?? new OneWeigher<TKey, TValue>(); }
        }

        public int Weigh(TKey key, TValue value)
        {
            return 1;
        }
    }
}
