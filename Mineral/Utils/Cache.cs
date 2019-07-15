using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace Cache
{
    public class Cache<T> : IDisposable
    {
        #region Field
        private MemoryCache cache = null;
        private CacheItemPolicy cache_policy = new CacheItemPolicy();
        #endregion


        #region Property
        public TimeSpan ExpiredTime { get; set; }
        public long MaxCapacity { get; set; }

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
            policy.AbsoluteExpiration = DateTime.UtcNow + ExpiredTime;
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
            if (MaxCapacity > 0 && this.cache.GetCount() < MaxCapacity)
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

        public void Dispose()
        {
            this.cache.Dispose();
        }
        #endregion
    }
}
