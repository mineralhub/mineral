/*
 * Copyright (c) [2016] [ <ether.camp> ]
 * This file is part of the ethereumJ library.
 *
 * The ethereumJ library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The ethereumJ library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the ethereumJ library. If not, see <http://www.gnu.org/licenses/>.
 */

using Org.BouncyCastle.Math;
/**
* Definition of {@link BN128} over F_p2, where "p" equals {@link Params#P} <br/>
*
* Curve equation: <br/> Y^2 = X^3 + b, where "b" equals {@link Params#B_Fp2} <br/>
*
* @author Mikhail Kalinin
* @since 31.08.2017
*/
namespace Mineral.Cryptography.zksnark
{
    public class BN128Fp2 : BN128<Fp2>
    {
        // the point at infinity
        public static readonly BN128<Fp2> ZERO = new BN128Fp2(Fp2.ZERO, Fp2.ZERO, Fp2.ZERO);

        protected BN128Fp2(Fp2 x, Fp2 y, Fp2 z)
            : base(x, y, z)
        {
        }
        protected BN128Fp2(BigInteger a, BigInteger b, BigInteger c, BigInteger d)
        : base(Fp2.Create(a, b), Fp2.Create(c, d), Fp2._1)
        {
        }

        protected override BN128<Fp2> Zero()
        {
            return ZERO;
        }

        protected override BN128<Fp2> Instance(Fp2 x, Fp2 y, Fp2 z)
        {
            return new BN128Fp2(x, y, z);
        }

        protected override Fp2 B()
        {
            return Parameters.B_Fp2;
        }

        protected override Fp2 One()
        {
            return Fp2._1;
        }

        /**
         * Checks whether provided data are coordinates of a point on the curve, then checks if this point
         * is a member of subgroup of order "r" and if checks have been passed it returns a point,
         * otherwise returns null
         */
        public static BN128<Fp2> Create(byte[] aa, byte[] bb, byte[] cc, byte[] dd)
        {
            Fp2 x = Fp2.Create(aa, bb);
            Fp2 y = Fp2.Create(cc, dd);

            // check for point at infinity
            if (x.IsZero() && y.IsZero())
            {
                return ZERO;
            }

            BN128<Fp2> p = new BN128Fp2(x, y, Fp2._1);

            // check whether point is a valid one
            if (p.IsValid())
            {
                return p;
            }
            else
            {
                return null;
            }
        }
    }
}
