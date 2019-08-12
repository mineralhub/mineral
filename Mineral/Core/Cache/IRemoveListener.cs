using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache
{
    public interface IRemovalListener<TKey, TValue>
    {
        void OnRemoval(RemovalNotification<TKey, TValue> notification);
    }

    public class NullListener<TKey, TValue> : IRemovalListener<TKey, TValue>
    {
        private static NullListener<TKey, TValue> instance = null;

        public static NullListener<TKey, TValue> Instance
        {
            get { return instance ?? new NullListener<TKey, TValue>(); }
        }

        public void OnRemoval(RemovalNotification<TKey, TValue> notification)
        {
        }
    }
}
