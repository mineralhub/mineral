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
* Implementation of specific cyclic subgroup of points belonging to {@link BN128Fp2} <br/> Members
* of this subgroup are passed as a second param to pairing input {@link
* PairingCheck#addPair(BN128G1, BN128G2)} <br/> <br/>
*
* The order of subgroup is {@link Params#R} <br/> Generator of subgroup G = <br/>
* (11559732032986387107991004021392285783925812861821192530917403151452391805634 * i + <br/>
* 10857046999023057135944570762232829481370756359578518086990519993285655852781, <br/>
* 4082367875863433681332203403145435568316851327593401208105741076214120093531 * i + <br/>
* 8495653923123431417604973247489272438418190587263600148770280649306958101930) <br/> <br/>
*
* @author Mikhail Kalinin
* @since 31.08.2017
*/
namespace Mineral.Cryptography.zksnark
{
    public class BN128G2 : BN128Fp2
    {
        public static readonly BigInteger FR_NEG_ONE = BigInteger.One.Negate().Mod(Parameters.R);

        public BN128G2(BN128<Fp2> p) : base(p.x, p.y, p.z) { }
        public BN128G2(Fp2 x, Fp2 y, Fp2 z) : base(x, y, z) { }
        public BN128G2 ToAffineBN128G2()
        {
            return new BN128G2(base.ToAffine());
        }

        /**
         * Checks whether provided data are coordinates of a point belonging to subgroup, if check has
         * been passed it returns a point, otherwise returns null
         */
        public static BN128G2 Create(byte[] a, byte[] b, byte[] c, byte[] d)
        {
            BN128<Fp2> p = BN128Fp2.Create(a, b, c, d);

            // fails if point is invalid
            if (p == null)
            {
                return null;
            }

            // check whether point is a subgroup member
            if (!IsGroupMember(p))
            {
                return null;
            }

            return new BN128G2(p);
        }

        private static bool IsGroupMember(BN128<Fp2> p)
        {
            BN128<Fp2> left = p.Mul(FR_NEG_ONE).Add(p);
            return left.IsZero(); // should satisfy condition: -1 * p + p == 0, where -1 belongs to F_r
        }

        public BN128G2 MulByP()
        {
            Fp2 rx = Parameters.TWIST_MUL_BY_P_X.Mul(x.FrobeniusMap(1));
            Fp2 ry = Parameters.TWIST_MUL_BY_P_Y.Mul(y.FrobeniusMap(1));
            Fp2 rz = z.FrobeniusMap(1);

            return new BN128G2(rx, ry, rz);
        }
    }
}
