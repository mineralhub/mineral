using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Common
{
    public class Wrapper<T>
    {
        #region Field
        private Equivalence<T> equivalence;
        private T reference;
        #endregion


        #region Property
        #endregion


        #region Constructor
        private Wrapper(Equivalence<T> equivalence, T reference)
        {
            this.equivalence = equivalence ?? throw new ArgumentNullException();
            this.reference = reference;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public T Get()
        {
            return this.reference;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is Wrapper<T>))
            {
                return false;
            }

            Wrapper<T> other = (Wrapper<T>)obj;
            if (this.equivalence.Equals(other.equivalence))
            {
                Equivalence<T> equivalence = (Equivalence<T>)this.equivalence;

                return this.equivalence.Equivalent(this.reference, other.reference);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.equivalence.Hash(this.reference);
        }

        public override string ToString()
        {
            return this.equivalence + ".wrap(" + this.reference + ")";
        }
        #endregion
    }
}
