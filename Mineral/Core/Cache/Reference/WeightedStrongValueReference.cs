using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Reference
{
    public class WeightedStrongValueReference<TKey, TValue> : StrongValueReference<TKey, TValue>
    {
        #region Field
        private int weight = 0;
        #endregion


        #region Property
        public override int Weight
        {
            get { return this.weight; }
        }
        #endregion


        #region Contructor
        public WeightedStrongValueReference(TValue referent, int weight)
            : base (referent)
        {
            this.weight = weight;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
