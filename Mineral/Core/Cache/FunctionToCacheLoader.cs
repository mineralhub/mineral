using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mineral.Core.Cache
{
    public class FunctionToCacheLoader<TKey, TValue> : CacheLoader<TKey, TValue>
    {
        #region Field
        private Func<TKey, TValue> func_compute;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public FunctionToCacheLoader(Func<TKey, TValue> function)
        {
            this.func_compute = function ?? throw new ArgumentNullException();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override TValue Load(TKey key)
        {
            return func_compute(key);
        }
        #endregion
    }
}
