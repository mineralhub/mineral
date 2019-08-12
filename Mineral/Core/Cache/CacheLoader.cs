using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mineral.Core.Cache
{
    public abstract class CacheLoader<TKey, TValue>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        protected CacheLoader() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public abstract TValue Load(TKey key);

        public Task<TValue> Relaod(TKey key, TValue old_value)
        {
            if (key == null)
                throw new ArgumentNullException("Reload key is null");

            if (old_value == null)
                throw new ArgumentNullException("Reload old value is null");

            return Task.Run<TValue>(() =>
            {
                return Load(key);
            });
        }

        public static CacheLoader<TKey, TValue> From(Func<TKey, TValue> function)
        {
            return new FunctionToCacheLoader<TKey, TValue>(function);
        }
        #endregion
    }
}
