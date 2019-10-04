
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
using Org.BouncyCastle.Utilities;

/**
* Arithmetic in F_p2 <br/> <br/>
*
* "p" equals 21888242871839275222246405745257275088696311157297823662689037894645226208583,
* elements of F_p2 are represented as a polynomials "a * i + b" modulo "i^2 + 1" from the ring
* F_p[i] <br/> <br/>
*
* Field arithmetic is ported from <a href="https://github.com/scipr-lab/libff/blob/master/libff/algebra/fields/fp2.tcc">libff</a>
* <br/>
*
* @author Mikhail Kalinin
* @since 01.09.2017
*/
namespace Mineral.Cryptography.zksnark
{
    public class Fp2 : IField<Fp2>
    {
        public static readonly Fp2 ZERO = new Fp2(Fp.ZERO, Fp.ZERO);
        public static readonly Fp2 _1 = new Fp2(Fp._1, Fp.ZERO);
        public static readonly Fp2 NON_RESIDUE = new Fp2(BigInteger.ValueOf(9), BigInteger.One);

        public static readonly Fp[] FROBENIUS_COEFFS_B = new Fp[]{
                              new Fp(BigInteger.One),
                              new Fp(new BigInteger("21888242871839275222246405745257275088696311157297823662689037894645226208582"))
                              };
    
        public Fp a;
        public Fp b;

        public Fp2(Fp a, Fp b)
        {
            this.a = a;
            this.b = b;
        }

        public Fp2(BigInteger a, BigInteger b)
             : this(new Fp(a), new Fp(b))
        {
        }

        public Fp2 Squared()
        {
            Fp ab = a.Mul(b);

            Fp ra = a.Add(b).Mul(b.Mul(Fp.NON_RESIDUE).Add(a))
                .Sub(ab)
                .Sub(ab.Mul(Fp.NON_RESIDUE)); // ra = (a + b)(a + NON_RESIDUE * b) - ab - NON_RESIDUE * b
            Fp rb = ab.Dbl();

            return new Fp2(ra, rb);
        }

        public Fp2 Mul(Fp2 o)
        {
            Fp aa = a.Mul(o.a);
            Fp bb = b.Mul(o.b);

            Fp ra = bb.Mul(Fp.NON_RESIDUE).Add(aa);    // ra = a1 * a2 + NON_RESIDUE * b1 * b2
            Fp rb = a.Add(b).Mul(o.a.Add(o.b)).Sub(aa).Sub(bb);     // rb = (a1 + b1)(a2 + b2) - a1 * a2 - b1 * b2

            return new Fp2(ra, rb);
        }

        public Fp2 Add(Fp2 o)
        {
            return new Fp2(a.Add(o.a), b.Add(o.b));
        }

        public Fp2 Sub(Fp2 o)
        {
            return new Fp2(a.Sub(o.a), b.Sub(o.b));
        }

        public Fp2 Dbl()
        {
            return this.Add(this);
        }

        public Fp2 Inverse()
        {

            Fp t0 = a.Squared();
            Fp t1 = b.Squared();
            Fp t2 = t0.Sub(Fp.NON_RESIDUE.Mul(t1));
            Fp t3 = t2.Inverse();

            Fp ra = a.Mul(t3);          // ra = a * t3
            Fp rb = b.Mul(t3).Negate(); // rb = -(b * t3)

            return new Fp2(ra, rb);
        }

        public Fp2 Negate()
        {
            return new Fp2(a.Negate(), b.Negate());
        }

        public bool IsZero()
        {
            return this.Equals(ZERO);
        }

        public bool IsValid()
        {
            return a.IsValid() && b.IsValid();
        }

        public static Fp2 Create(BigInteger aa, BigInteger bb)
        {
            Fp a = Fp.Create(aa);
            Fp b = Fp.Create(bb);

            return new Fp2(a, b);
        }

        public static Fp2 Create(byte[] aa, byte[] bb)
        {
            Fp a = Fp.Create(aa);
            Fp b = Fp.Create(bb);

            return new Fp2(a, b);
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            Fp2 fp2 = (Fp2)o;

            if (a != null ? !a.Equals(fp2.a) : fp2.a != null)
            {
                return false;
            }
            return !(b != null ? !b.Equals(fp2.b) : fp2.b != null);

        }

        public override int GetHashCode()
        {
            return (a.GetHashCode() + b.GetHashCode()).GetHashCode();
        }

        public Fp2 FrobeniusMap(int power)
        {

            Fp ra = a;
            Fp rb = FROBENIUS_COEFFS_B[power % 2].Mul(b);

            return new Fp2(ra, rb);
        }

        public Fp2 MulByNonResidue()
        {
            return NON_RESIDUE.Mul(this);
        }

        public override string ToString()
        {
            return string.Format("%si + %s", a.ToString(), b.ToString());
        }
    }
}

