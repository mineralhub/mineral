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
* Implementation of Barreto–Naehrig curve defined over abstract finite field. This curve is one of
* the keys to zkSNARKs. <br/> This specific curve was introduced in <a
* href="https://github.com/scipr-lab/libff#elliptic-curve-choices">libff</a> and used by a proving
* system in <a href="https://github.com/zcash/zcash/wiki/specification#zcash-protocol">ZCash
* protocol</a> <br/> <br/>
*
* Curve equation: <br/> Y^2 = X^3 + b, where "b" is a constant number belonging to corresponding
* specific field <br/> Point at infinity is encoded as <code>(0, 0, 0)</code> <br/> <br/>
*
* This curve has embedding degree 12 with respect to "r" (see {@link Params#R}), which means that
* "r" is a multiple of "p ^ 12 - 1", this condition is important for pairing operation implemented
* in {@link PairingCheck}<br/> <br/>
*
* Code of curve arithmetic has been ported from <a href="https://github.com/scipr-lab/libff/blob/master/libff/algebra/curves/alt_bn128/alt_bn128_g1.cpp">libff</a>
* <br/> <br/>
*
* Current implementation uses Jacobian coordinate system as <a href="https://github.com/scipr-lab/libff/blob/master/libff/algebra/curves/alt_bn128/alt_bn128_g1.cpp">libff</a>
* does, use {@link #toEthNotation()} to convert Jacobian coords to Ethereum encoding <br/>
*
* @author Mikhail Kalinin
* @since 05.09.2017
*/
namespace Mineral.Cryptography.zksnark
{
    public abstract class BN128<T> where T : IField<T>
    {
       public T x;
       public T y;
       public T z;

        protected BN128(T x, T y, T z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        /**
         * Point at infinity in Ethereum notation: should return (0; 0; 0), {@link #isZero()} method
         * called for that point, also, returns {@code true}
         */
        protected abstract BN128<T> Zero();
        protected abstract BN128<T> Instance(T x, T y, T z);
        protected abstract T B();
        protected abstract T One();

        /**
         * Transforms given Jacobian to affine coordinates and then creates a point
         */
        public virtual BN128<T> ToAffine()
        {
            if (IsZero())
            {
                BN128<T> zero = Zero();
                return Instance(zero.x, One(), zero.z); // (0; 1; 0)
            }

            T zInv = z.Inverse();
            T zInv2 = zInv.Squared();
            T zInv3 = zInv2.Mul(zInv);

            T ax = x.Mul(zInv2);
            T ay = y.Mul(zInv3);

            return Instance(ax, ay, One());
        }

        /**
         * Runs affine transformation and encodes point at infinity as (0; 0; 0)
         */
        public BN128<T> ToEthNotation()
        {
            BN128<T> affine = ToAffine();

            // affine zero is (0; 1; 0), convert to Ethereum zero: (0; 0; 0)
            if (affine.IsZero())
            {
                return Zero();
            }
            else
            {
                return affine;
            }
        }

        protected bool IsOnCurve()
        {
            if (IsZero())
            {
                return true;
            }

            T z6 = z.Squared().Mul(z).Squared();

            T left = y.Squared();                          // y^2
            T right = x.Squared().Mul(x).Add(B()).Mul(z6);  // x^3 + b * z^6
            return left.Equals(right);
        }

        public BN128<T> Add(BN128<T> o)
        {
            if (this.IsZero())
            {
                return o; // 0 + P = P
            }
            if (o.IsZero())
            {
                return this; // P + 0 = P
            }

            T x1 = this.x, y1 = this.y, z1 = this.z;
            T x2 = o.x, y2 = o.y, z2 = o.z;

            // ported code is started from here
            // next calculations are done in Jacobian coordinates

            T z1z1 = z1.Squared();
            T z2z2 = z2.Squared();

            T u1 = x1.Mul(z2z2);
            T u2 = x2.Mul(z1z1);

            T z1Cubed = z1.Mul(z1z1);
            T z2Cubed = z2.Mul(z2z2);

            T s1 = y1.Mul(z2Cubed);      // s1 = y1 * Z2^3
            T s2 = y2.Mul(z1Cubed);      // s2 = y2 * Z1^3

            if (u1.Equals(u2) && s1.Equals(s2))
            {
                return Dbl(); // P + P = 2P
            }

            T h = u2.Sub(u1);          // h = u2 - u1
            T i = h.Dbl().Squared();   // i = (2 * h)^2
            T j = h.Mul(i);            // j = h * i
            T r = s2.Sub(s1).Dbl();    // r = 2 * (s2 - s1)
            T v = u1.Mul(i);           // v = u1 * i
            T zz = z1.Add(z2).Squared()
                .Sub(z1.Squared()).Sub(z2.Squared());

            T x3 = r.Squared().Sub(j).Sub(v.Dbl());        // x3 = r^2 - j - 2 * v
            T y3 = v.Sub(x3).Mul(r).Sub(s1.Mul(j).Dbl());  // y3 = r * (v - x3) - 2 * (s1 * j)
            T z3 = zz.Mul(h); // z3 = ((z1+z2)^2 - z1^2 - z2^2) * h = zz * h

            return Instance(x3, y3, z3);
        }

        public BN128<T> Mul(BigInteger s)
        {
            if (s.CompareTo(BigInteger.Zero) == 0) // P * 0 = 0
            {
                return Zero();
            }

            if (IsZero())
            {
                return this; // 0 * s = 0
            }

            BN128<T> res = Zero();

            for (int i = s.BitLength - 1; i >= 0; i--)
            {

                res = res.Dbl();

                if (s.TestBit(i))
                {
                    res = res.Add(this);
                }
            }

            return res;
        }

        private BN128<T> Dbl()
        {
            if (IsZero())
            {
                return this;
            }

            // ported code is started from here
            // next calculations are done in Jacobian coordinates with z = 1

            T a = x.Squared();     // a = x^2
            T b = y.Squared();     // b = y^2
            T c = b.Squared();     // c = b^2
            T d = x.Add(b).Squared().Sub(a).Sub(c);
            d = d.Add(d);                              // d = 2 * ((x + b)^2 - a - c)
            T e = a.Add(a).Add(a);  // e = 3 * a
            T f = e.Squared();     // f = e^2

            T x3 = f.Sub(d.Add(d)); // rx = f - 2 * d
            T y3 = e.Mul(d.Sub(x3)).Sub(c.Dbl().Dbl().Dbl()); // ry = e * (d - rx) - 8 * c
            T z3 = y.Mul(z).Dbl(); // z3 = 2 * y * z

            return Instance(x3, y3, z3);
        }

        public bool IsZero()
        {
            return z.IsZero();
        }

        public bool IsValid()
        {
            // check whether coordinates belongs to the Field
            if (!x.IsValid() || !y.IsValid() || !z.IsValid())
            {
                return false;
            }

            // check whether point is on the curve
            if (!IsOnCurve())
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Format("({0}; {1}; {2})", x.ToString(), y.ToString(), z.ToString());
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (!(o is BN128<T>)) {
                return false;
            }

            BN128<T> bn128 = o as BN128<T>;
            if (x != null ? !x.Equals(bn128.x) : bn128.x != null)
            {
                return false;
            }
            if (y != null ? !y.Equals(bn128.y) : bn128.y != null)
            {
                return false;
            }
            return !(z != null ? !z.Equals(bn128.z) : bn128.z != null);
        }
    }

}
