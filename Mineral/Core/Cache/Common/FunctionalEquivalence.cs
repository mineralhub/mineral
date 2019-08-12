using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Common
{
    public class FunctionalEquivalence<T, U> : Equivalence<T>
        where T : U
    {
        #region Field
        private readonly Func<T, T> function;
        private readonly Equivalence<U> result_equivalence;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public FunctionalEquivalence(Func<T, T> function, Equivalence<U> result_equivalence)
        {
            if (function == null || result_equivalence == null)
                throw new ArgumentNullException();

            this.function = function;
            this.result_equivalence = result_equivalence;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected override bool DoEquivalent(T t, T u)
        {
            return this.result_equivalence.Equivalent(function(t), function(u));
        }

        protected override int DoHash(T t)
        {
            return this.result_equivalence.Hash(function(t));
        }
        #endregion


        #region External Method
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is FunctionalEquivalence<T, U>)
            {
                FunctionalEquivalence<T, U> other = (FunctionalEquivalence<T, U>)obj;
                return this.function.Equals(other.function) && this.result_equivalence.Equals(other.result_equivalence);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() + this.function.GetHashCode() + this.result_equivalence.GetHashCode();
        }

        public override string ToString()
        {
            return this.result_equivalence + ".OnResultOf(" + this.function + ")";
        }
        #endregion
    }
}
