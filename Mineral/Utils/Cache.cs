using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace Mineral.Utils
{
    public class Cache<T> : IEnumerable<KeyValuePair<string, T>>, IDisposable
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
        public Cache(string name)
        {
            this.cache = new MemoryCache(name);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private CacheItemPolicy GetPolicy()
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + this.expire_time;

            return policy;
        }
        #endregion


        #region External Method
        public bool Add(string key, T value)
        {
            if (this.cache.GetCount() >= this.max_capacity)
                return false;

            if (!this.cache.Contains(key))
            {
                this.cache.Add(key, value, GetPolicy());
            }
            else
            {
                this.cache.Set(key, value, GetPolicy());
            }

            return true;
        }

        public void Set(string key, T value)
        {
            if (this.cache.Contains(key))
            {
                this.cache.Set(key, value, GetPolicy());
            }
        }

        public object Get(string key)
        {
            return this.cache.Get(key);
        }

        public void Remove(string key)
        {
            if (this.cache.Contains(key))
            {
                this.cache.Remove(key);
            }
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

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            foreach (var entry in cache)
            {
                yield return new KeyValuePair<string, T>(entry.Key, (T)entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
