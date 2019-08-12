using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Common
{
    public abstract class Equivalence<T>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected abstract bool DoEquivalent(T t, T u);
        protected abstract int DoHash(T t);
        #endregion


        #region External Method
        public bool Test(T t, T u)
        {
            return Equivalent(t, u);
        }

        public bool Equivalent(T a, T b)
        {
            if (a.Equals(b))
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }

            return DoEquivalent(a, b);
        }

        public int Hash(T t)
        {
            if (t == null)
            {
                return 0;
            }
            return DoHash(t);
        }

        public Equivalence<T1> OnResultOf<T1>(Func<T1, T1> function)
            where T1 : T
        {
            return new FunctionalEquivalence<T1, T>(function, this);
        }

        public Wrapper<T> Wrap(T reference)
        {
            return new Wrapper<T>(this, reference);
        }
        #endregion
    }
}
