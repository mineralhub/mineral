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


using System.Collections.Generic;
using Org.BouncyCastle.Math;
/**
* Implementation of a Pairing Check operation over points of two twisted Barreto–Naehrig curves
* {@link BN128Fp}, {@link BN128Fp2}<br/> <br/>
*
* The Pairing itself is a transformation of the form G1 x G2 -> Gt, <br/> where G1 and G2 are
* members of {@link BN128G1} {@link BN128G2} respectively, <br/> Gt is a subgroup of roots of unity
* in {@link Fp12} field, root degree equals to {@link Params#R} <br/> <br/>
*
* Pairing Check input is a sequence of point pairs, the result is either 1 or 0, 1 is considered as
* success, 0 as fail <br/> <br/>
*
* Usage: <ul> <li>add pairs sequentially with {@link #addPair(BN128G1, BN128G2)}</li> <li>run check
* with {@link #run()} after all paris have been added</li> <li>get result with {@link
* #result()}</li> </ul>
*
* Arithmetic has been ported from <a href="https://github.com/scipr-lab/libff/blob/master/libff/algebra/curves/alt_bn128/alt_bn128_pairing.cpp">libff</a>
* Ate pairing algorithms
*
* @author Mikhail Kalinin
* @since 01.09.2017
*/
namespace Mineral.Cryptography.zksnark
{
    public class PairingCheck
    {
        public static readonly BigInteger LOOP_COUNT = new BigInteger("29793968203157093288");

        private List<Pair> pairs = new List<Pair>();
        private Fp12 product = Fp12._1;

        private PairingCheck()
        {
        }

        public static PairingCheck Create()
        {
            return new PairingCheck();
        }

        public void AddPair(BN128G1 g1, BN128G2 g2)
        {
            pairs.Add(Pair.Of(g1, g2));
        }

        public void Run()
        {
            foreach (Pair pair in pairs)
            {
                Fp12 miller = pair.MillerLoop();

                if (!miller.Equals(Fp12._1))    // run mul code only if necessary
                {
                    product = product.Mul(miller);
                }
            }

            // finalize
            product = FinalExponentiation(product);
        }

        public int Result()
        {
            return product.Equals(Fp12._1) ? 1 : 0;
        }

        private static Fp12 MillerLoop(BN128G1 g1, BN128G2 g2)
        {
            // convert to affine coordinates
            g1 = g1.ToAffineBN128G1();
            g2 = g2.ToAffineBN128G2();

            // calculate Ell coefficients
            List<EllCoeffs> coeffs = CalcEllCoeffs(g2);

            Fp12 f = Fp12._1;
            int idx = 0;

            EllCoeffs c;
            // for each bit except most significant one
            for (int i = LOOP_COUNT.BitLength - 2; i >= 0; i--)
            {
                c = coeffs[idx++];
                f = f.Squared();
                f = f.MulBy024(c.ell0, g1.y.Mul(c.ellVW), g1.x.Mul(c.ellVV));

                if (LOOP_COUNT.TestBit(i))
                {
                    c = coeffs[idx++];
                    f = f.MulBy024(c.ell0, g1.y.Mul(c.ellVW), g1.x.Mul(c.ellVV));
                }

            }

            c = coeffs[idx++];
            f = f.MulBy024(c.ell0, g1.y.Mul(c.ellVW), g1.x.Mul(c.ellVV));

            c = coeffs[idx];
            f = f.MulBy024(c.ell0, g1.y.Mul(c.ellVW), g1.x.Mul(c.ellVV));

            return f;
        }

        private static List<EllCoeffs> CalcEllCoeffs(BN128G2 base_val)
        {
            List<EllCoeffs> coeffs = new List<EllCoeffs>();

            BN128G2 addend = base_val;
            Precomputed addition;
            // for each bit except most significant one
            for (int i = LOOP_COUNT.BitLength - 2; i >= 0; i--)
            {

                Precomputed doubling = FlippedMillerLoopDoubling(addend);

                addend = doubling.g2;
                coeffs.Add(doubling.coeffs);

                if (LOOP_COUNT.TestBit(i))
                {
                    addition = FlippedMillerLoopMixedAddition(base_val, addend);
                    addend = addition.g2;
                    coeffs.Add(addition.coeffs);
                }
            }

            BN128G2 q1 = base_val.MulByP();
            BN128G2 q2 = q1.MulByP();

            q2 = new BN128G2(q2.x, q2.y.Negate(), q2.z); // q2.y = -q2.y

            addition = FlippedMillerLoopMixedAddition(q1, addend);
            addend = addition.g2;
            coeffs.Add(addition.coeffs);

            addition = FlippedMillerLoopMixedAddition(q2, addend);
            coeffs.Add(addition.coeffs);

            return coeffs;
        }

        private static Precomputed FlippedMillerLoopMixedAddition(BN128G2 base_val, BN128G2 addend)
        {

            Fp2 x1 = addend.x, y1 = addend.y, z1 = addend.z;
            Fp2 x2 = base_val.x, y2 = base_val.y;

            Fp2 d = x1.Sub(x2.Mul(z1));             // d = x1 - x2 * z1
            Fp2 e = y1.Sub(y2.Mul(z1));             // e = y1 - y2 * z1
            Fp2 f = d.Squared();                    // f = d^2
            Fp2 g = e.Squared();                    // g = e^2
            Fp2 h = d.Mul(f);                       // h = d * f
            Fp2 i = x1.Mul(f);                      // i = x1 * f
            Fp2 j = h.Add(z1.Mul(g)).Sub(i.Dbl());  // j = h + z1 * g - 2 * i

            Fp2 x3 = d.Mul(j);                           // x3 = d * j
            Fp2 y3 = e.Mul(i.Sub(j)).Sub(h.Mul(y1));     // y3 = e * (i - j) - h * y1)
            Fp2 z3 = z1.Mul(h);                          // z3 = Z1*H

            Fp2 ell0 = Parameters.TWIST.Mul(e.Mul(x2).Sub(d.Mul(y2)));     // ell_0 = TWIST * (e * x2 - d * y2)
            Fp2 ellVV = e.Negate();                             // ell_VV = -e
            Fp2 ellVW = d;                                      // ell_VW = d

            return Precomputed.Of(
                new BN128G2(x3, y3, z3),
                new EllCoeffs(ell0, ellVW, ellVV)
            );
        }

        private static Precomputed FlippedMillerLoopDoubling(BN128G2 g2)
        {
            Fp2 x = g2.x, y = g2.y, z = g2.z;

            Fp2 a = Fp._2_INV.Mul(x.Mul(y));            // a = x * y / 2
            Fp2 b = y.Squared();                        // b = y^2
            Fp2 c = z.Squared();                        // c = z^2
            Fp2 d = c.Add(c).Add(c);                    // d = 3 * c
            Fp2 e = Parameters.B_Fp2.Mul(d);                       // e = twist_b * d
            Fp2 f = e.Add(e).Add(e);                    // f = 3 * e
            Fp2 g = Fp._2_INV.Mul(b.Add(f));            // g = (b + f) / 2
            Fp2 h = y.Add(z).Squared().Sub(b.Add(c));   // h = (y + z)^2 - (b + c)
            Fp2 i = e.Sub(b);                           // i = e - b
            Fp2 j = x.Squared();                        // j = x^2
            Fp2 e2 = e.Squared();                       // e2 = e^2

            Fp2 rx = a.Mul(b.Sub(f));                       // rx = a * (b - f)
            Fp2 ry = g.Squared().Sub(e2.Add(e2).Add(e2));   // ry = g^2 - 3 * e^2
            Fp2 rz = b.Mul(h);                              // rz = b * h

            Fp2 ell0 = Parameters.TWIST.Mul(i);        // ell_0 = twist * i
            Fp2 ellVW = h.Negate();         // ell_VW = -h
            Fp2 ellVV = j.Add(j).Add(j);    // ell_VV = 3 * j

            return Precomputed.Of(
                new BN128G2(rx, ry, rz),
                new EllCoeffs(ell0, ellVW, ellVV)
            );
        }

        public static Fp12 FinalExponentiation(Fp12 el)
        {
            // first chunk
            Fp12 w = new Fp12(el.a, el.b.Negate()); // el.b = -el.b
            Fp12 x = el.Inverse();
            Fp12 y = w.Mul(x);
            Fp12 z = y.FrobeniusMap(2);
            Fp12 pre = z.Mul(y);

            // last chunk
            Fp12 a = pre.NegExp(Parameters.PAIRING_FINAL_EXPONENT_Z);
            Fp12 b = a.CyclotomicSquared();
            Fp12 c = b.CyclotomicSquared();
            Fp12 d = c.Mul(b);
            Fp12 e = d.NegExp(Parameters.PAIRING_FINAL_EXPONENT_Z);
            Fp12 f = e.CyclotomicSquared();
            Fp12 g = f.NegExp(Parameters.PAIRING_FINAL_EXPONENT_Z);
            Fp12 h = d.UnitaryInverse();
            Fp12 i = g.UnitaryInverse();
            Fp12 j = i.Mul(e);
            Fp12 k = j.Mul(h);
            Fp12 l = k.Mul(b);
            Fp12 m = k.Mul(e);
            Fp12 n = m.Mul(pre);
            Fp12 o = l.FrobeniusMap(1);
            Fp12 p = o.Mul(n);
            Fp12 q = k.FrobeniusMap(2);
            Fp12 r = q.Mul(p);
            Fp12 s = pre.UnitaryInverse();
            Fp12 t = s.Mul(l);
            Fp12 u = t.FrobeniusMap(3);
            Fp12 v = u.Mul(r);

            return v;
        }

        public class Precomputed
        {
            public BN128G2 g2;
            public EllCoeffs coeffs;

            public static Precomputed Of(BN128G2 g2, EllCoeffs coeffs)
            {
                return new Precomputed(g2, coeffs);
            }

            public Precomputed(BN128G2 g2, EllCoeffs coeffs)
            {
                this.g2 = g2;
                this.coeffs = coeffs;
            }
        }

        public class Pair
        {
            public BN128G1 g1;
            public BN128G2 g2;

            public static Pair Of(BN128G1 g1, BN128G2 g2)
            {
                return new Pair(g1, g2);
            }

            public Pair(BN128G1 g1, BN128G2 g2)
            {
                this.g1 = g1;
                this.g2 = g2;
            }

            public Fp12 MillerLoop()
            {

                // miller loop result equals "1" if at least one of the points is zero
                if (g1.IsZero())
                {
                    return Fp12._1;
                }
                if (g2.IsZero())
                {
                    return Fp12._1;
                }

                return PairingCheck.MillerLoop(g1, g2);
            }
        }

        public class EllCoeffs
        {
            public Fp2 ell0;
            public Fp2 ellVW;
            public Fp2 ellVV;

            public EllCoeffs(Fp2 ell0, Fp2 ellVW, Fp2 ellVV)
            {
                this.ell0 = ell0;
                this.ellVW = ellVW;
                this.ellVV = ellVV;
            }
        }
    }
}
