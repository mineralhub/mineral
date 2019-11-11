using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;

namespace Mineral.Core.Witness
{
    public class WitnessSortComparer : IComparer<WitnessCapsule>
    {
        public int Compare(WitnessCapsule witness1, WitnessCapsule witness2)
        {
            int result = 0;

            if (witness1.VoteCount > witness2.VoteCount)
            {
                result = 1;
            }
            else if (witness1.VoteCount < witness2.VoteCount)
            {
                result = -1;
            }
            else
            {
                if (witness1.GetHashCode() > witness2.GetHashCode())
                {
                    result = 1;
                }
                else if (witness1.GetHashCode() < witness2.GetHashCode())
                {
                    result = -1;
                }
                else
                {
                    result = 0;
                }
            }

            return result;
        }
    }
}
