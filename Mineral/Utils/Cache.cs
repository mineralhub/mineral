using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace Mineral.Utils
{
    public class Cache<T> : IDisposable
    {
        #region Field
        private MemoryCache cache = null;
        private TimeSpan expire_time = TimeSpan.FromTicks(0);
        private long max_capacity = long.MaxValue;
        #endregion


        #region Property
        public long Count
        {
            get { return this.cache.GetCount(); }
        }
        #endregion


        #region Contructor
        public Cache()
        {
            this.cache = MemoryCache.Default;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private CacheItemPolicy GetPolicy()
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow + this.expire_time;
            policy.RemovedCallback = new CacheEntryRemovedCallback((CacheEntryRemovedArguments arg) =>
            {
                Console.WriteLine("Remove : " + arg.CacheItem.Key);
            });

            return policy;
        }
        #endregion


        #region External Method
        public bool Add(string key, T value)
        {
            if (this.cache.GetCount() < this.max_capacity)
                return false;

            this.cache.Add(key, value, GetPolicy());

            return true;
        }

        public void Set(string key, T value)
        {
            this.cache.Set(key, value, GetPolicy());
        }

        public T Get(string key)
        {
            return (T)this.cache.Get(key);
        }

        public void Remove(string key)
        {
            this.cache.Remove(key);
        }

        public Cache<T> MaxCapacity(long capacity)
        {
            this.max_capacity = capacity;
            return this;
        }

        public Cache<T> ExpireTime(TimeSpan time)
        {
            this.expire_time = time;
            return this;
        }

        public void Dispose()
        {
            this.cache.Dispose();
        }
        #endregion
    }
}
