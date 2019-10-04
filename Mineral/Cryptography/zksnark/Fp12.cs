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
* Arithmetic in Fp_12 <br/> <br/>
*
* "p" equals 21888242871839275222246405745257275088696311157297823662689037894645226208583, <br/>
* elements of Fp_12 are represented with 2 elements of {@link Fp6} <br/> <br/>
*
* Field arithmetic is ported from <a href="https://github.com/scipr-lab/libff/blob/master/libff/algebra/fields/fp12_2over3over2.tcc">libff</a>
*
* @author Mikhail Kalinin
* @since 02.09.2017
*/
namespace Mineral.Cryptography.zksnark
{
    public class Fp12 : IField<Fp12>
    {
        public static readonly Fp12 ZERO = new Fp12(Fp6.ZERO, Fp6.ZERO);
        public static readonly Fp12 _1 = new Fp12(Fp6._1, Fp6.ZERO);

        static readonly Fp2[] FROBENIUS_COEFFS_B = new Fp2[]
{
                              new Fp2(BigInteger.One,
                                  BigInteger.Zero),

                              new Fp2(new BigInteger("8376118865763821496583973867626364092589906065868298776909617916018768340080"),
                                  new BigInteger("16469823323077808223889137241176536799009286646108169935659301613961712198316")),

                              new Fp2(new BigInteger("21888242871839275220042445260109153167277707414472061641714758635765020556617"),
                                  BigInteger.Zero),

                              new Fp2(new BigInteger("11697423496358154304825782922584725312912383441159505038794027105778954184319"),
                                  new BigInteger("303847389135065887422783454877609941456349188919719272345083954437860409601")),

                              new Fp2(new BigInteger("21888242871839275220042445260109153167277707414472061641714758635765020556616"),
                                  BigInteger.Zero),

                              new Fp2(new BigInteger("3321304630594332808241809054958361220322477375291206261884409189760185844239"),
                                  new BigInteger("5722266937896532885780051958958348231143373700109372999374820235121374419868")),

                              new Fp2(new BigInteger("21888242871839275222246405745257275088696311157297823662689037894645226208582"),
                                  BigInteger.Zero),

                              new Fp2(new BigInteger("13512124006075453725662431877630910996106405091429524885779419978626457868503"),
                                  new BigInteger("5418419548761466998357268504080738289687024511189653727029736280683514010267")),

                              new Fp2(new BigInteger("2203960485148121921418603742825762020974279258880205651966"),
                                  BigInteger.Zero),

                              new Fp2(new BigInteger("10190819375481120917420622822672549775783927716138318623895010788866272024264"),
                                  new BigInteger("21584395482704209334823622290379665147239961968378104390343953940207365798982")),

                              new Fp2(new BigInteger("2203960485148121921418603742825762020974279258880205651967"),
                                  BigInteger.Zero),

                              new Fp2(new BigInteger("18566938241244942414004596690298913868373833782006617400804628704885040364344"),
                                  new BigInteger("16165975933942742336466353786298926857552937457188450663314217659523851788715"))
};

        public Fp6 a;
        public Fp6 b;

        public Fp12(Fp6 a, Fp6 b)
        {
            this.a = a;
            this.b = b;
        }

        public Fp12 Squared()
        {
            Fp6 ab = a.Mul(b);

            Fp6 ra = a.Add(b).Mul(a.Add(b.MulByNonResidue())).Sub(ab).Sub(ab.MulByNonResidue());
            Fp6 rb = ab.Add(ab);

            return new Fp12(ra, rb);
        }

        public Fp12 Dbl()
        {
            return null;
        }

        public Fp12 MulBy024(Fp2 ell0, Fp2 ellVW, Fp2 ellVV)
        {
            Fp2 z0 = a.a;
            Fp2 z1 = a.b;
            Fp2 z2 = a.c;
            Fp2 z3 = b.a;
            Fp2 z4 = b.b;
            Fp2 z5 = b.c;

            Fp2 x0 = ell0;
            Fp2 x2 = ellVV;
            Fp2 x4 = ellVW;

            Fp2 t0, t1, t2, s0, t3, t4, d0, d2, d4, s1;

            d0 = z0.Mul(x0);
            d2 = z2.Mul(x2);
            d4 = z4.Mul(x4);
            t2 = z0.Add(z4);
            t1 = z0.Add(z2);
            s0 = z1.Add(z3).Add(z5);

            // For z.a_.a_ = z0.
            s1 = z1.Mul(x2);
            t3 = s1.Add(d4);
            t4 = Fp6.NON_RESIDUE.Mul(t3).Add(d0);
            z0 = t4;

            // For z.a_.b_ = z1
            t3 = z5.Mul(x4);
            s1 = s1.Add(t3);
            t3 = t3.Add(d2);
            t4 = Fp6.NON_RESIDUE.Mul(t3);
            t3 = z1.Mul(x0);
            s1 = s1.Add(t3);
            t4 = t4.Add(t3);
            z1 = t4;

            // For z.a_.c_ = z2
            t0 = x0.Add(x2);
            t3 = t1.Mul(t0).Sub(d0).Sub(d2);
            t4 = z3.Mul(x4);
            s1 = s1.Add(t4);
            t3 = t3.Add(t4);

            // For z.b_.a_ = z3 (z3 needs z2)
            t0 = z2.Add(z4);
            z2 = t3;
            t1 = x2.Add(x4);
            t3 = t0.Mul(t1).Sub(d2).Sub(d4);
            t4 = Fp6.NON_RESIDUE.Mul(t3);
            t3 = z3.Mul(x0);
            s1 = s1.Add(t3);
            t4 = t4.Add(t3);
            z3 = t4;

            // For z.b_.b_ = z4
            t3 = z5.Mul(x2);
            s1 = s1.Add(t3);
            t4 = Fp6.NON_RESIDUE.Mul(t3);
            t0 = x0.Add(x4);
            t3 = t2.Mul(t0).Sub(d0).Sub(d4);
            t4 = t4.Add(t3);
            z4 = t4;

            // For z.b_.c_ = z5.
            t0 = x0.Add(x2).Add(x4);
            t3 = s0.Mul(t0).Sub(s1);
            z5 = t3;

            return new Fp12(new Fp6(z0, z1, z2), new Fp6(z3, z4, z5));
        }

        public Fp12 Add(Fp12 o)
        {
            return new Fp12(a.Add(o.a), b.Add(o.b));
        }

        public Fp12 Mul(Fp12 o)
        {
            Fp6 a2 = o.a, b2 = o.b;
            Fp6 a1 = a, b1 = b;

            Fp6 a1a2 = a1.Mul(a2);
            Fp6 b1b2 = b1.Mul(b2);

            Fp6 ra = a1a2.Add(b1b2.MulByNonResidue());
            Fp6 rb = a1.Add(b1).Mul(a2.Add(b2)).Sub(a1a2).Sub(b1b2);

            return new Fp12(ra, rb);
        }

        public Fp12 Sub(Fp12 o)
        {
            return new Fp12(a.Sub(o.a), b.Sub(o.b));
        }

        public Fp12 Inverse()
        {
            Fp6 t0 = a.Squared();
            Fp6 t1 = b.Squared();
            Fp6 t2 = t0.Sub(t1.MulByNonResidue());
            Fp6 t3 = t2.Inverse();

            Fp6 ra = a.Mul(t3);
            Fp6 rb = b.Mul(t3).Negate();

            return new Fp12(ra, rb);
        }

        public Fp12 Negate()
        {
            return new Fp12(a.Negate(), b.Negate());
        }

        public bool IsZero()
        {
            return this.Equals(ZERO);
        }


        public bool IsValid()
        {
            return a.IsValid() && b.IsValid();
        }

        public Fp12 FrobeniusMap(int power)
        {
            Fp6 ra = a.FrobeniusMap(power);
            Fp6 rb = b.FrobeniusMap(power).Mul(FROBENIUS_COEFFS_B[power % 12]);

            return new Fp12(ra, rb);
        }

        public Fp12 CyclotomicSquared()
        {
            Fp2 z0 = a.a;
            Fp2 z4 = a.b;
            Fp2 z3 = a.c;
            Fp2 z2 = b.a;
            Fp2 z1 = b.b;
            Fp2 z5 = b.c;

            Fp2 t0, t1, t2, t3, t4, t5, tmp;

            // t0 + t1*y = (z0 + z1*y)^2 = a^2
            tmp = z0.Mul(z1);
            t0 = z0.Add(z1).Mul(z0.Add(Fp6.NON_RESIDUE.Mul(z1))).Sub(tmp).Sub(Fp6.NON_RESIDUE.Mul(tmp));
            t1 = tmp.Add(tmp);
            // t2 + t3*y = (z2 + z3*y)^2 = b^2
            tmp = z2.Mul(z3);
            t2 = z2.Add(z3).Mul(z2.Add(Fp6.NON_RESIDUE.Mul(z3))).Sub(tmp).Sub(Fp6.NON_RESIDUE.Mul(tmp));
            t3 = tmp.Add(tmp);
            // t4 + t5*y = (z4 + z5*y)^2 = c^2
            tmp = z4.Mul(z5);
            t4 = z4.Add(z5).Mul(z4.Add(Fp6.NON_RESIDUE.Mul(z5))).Sub(tmp).Sub(Fp6.NON_RESIDUE.Mul(tmp));
            t5 = tmp.Add(tmp);

            // for A

            // z0 = 3 * t0 - 2 * z0
            z0 = t0.Sub(z0);
            z0 = z0.Add(z0);
            z0 = z0.Add(t0);
            // z1 = 3 * t1 + 2 * z1
            z1 = t1.Add(z1);
            z1 = z1.Add(z1);
            z1 = z1.Add(t1);

            // for B

            // z2 = 3 * (xi * t5) + 2 * z2
            tmp = Fp6.NON_RESIDUE.Mul(t5);
            z2 = tmp.Add(z2);
            z2 = z2.Add(z2);
            z2 = z2.Add(tmp);

            // z3 = 3 * t4 - 2 * z3
            z3 = t4.Sub(z3);
            z3 = z3.Add(z3);
            z3 = z3.Add(t4);

            // for C

            // z4 = 3 * t2 - 2 * z4
            z4 = t2.Sub(z4);
            z4 = z4.Add(z4);
            z4 = z4.Add(t2);

            // z5 = 3 * t3 + 2 * z5
            z5 = t3.Add(z5);
            z5 = z5.Add(z5);
            z5 = z5.Add(t3);

            return new Fp12(new Fp6(z0, z4, z3), new Fp6(z2, z1, z5));
        }

        public Fp12 CyclotomicExp(BigInteger pow)
        {
            Fp12 res = _1;

            for (int i = pow.BitLength - 1; i >= 0; i--)
            {
                res = res.CyclotomicSquared();

                if (pow.TestBit(i))
                {
                    res = res.Mul(this);
                }
            }

            return res;
        }

        public Fp12 UnitaryInverse()
        {
            Fp6 ra = a;
            Fp6 rb = b.Negate();

            return new Fp12(ra, rb);
        }

        public Fp12 NegExp(BigInteger exp)
        {
            return this.CyclotomicExp(exp).UnitaryInverse();
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !(o.GetType() == GetType()))
            {
                return false;
            }

            Fp12 fp12 = (Fp12)o;

            if (a != null ? !a.Equals(fp12.a) : fp12.a != null)
            {
                return false;
            }
            return !(b != null ? !b.Equals(fp12.b) : fp12.b != null);

        }


        public override int GetHashCode()
        {
            return (a.GetHashCode() + b.GetHashCode()).GetHashCode();
        }


        public override string ToString()
        {
            return string.Format(
                "Fp12 ({0}; {1})\n" +
                    "     ({2}; {3})\n" +
                    "     ({4}; {5})\n" +
                    "     ({6}; {7})\n" +
                    "     ({8}; {9})\n" +
                    "     ({10}; {11})\n",

                a.a.a, a.a.b,
                a.b.a, a.b.b,
                a.c.a, a.c.b,
                b.a.a, b.a.b,
                b.b.a, b.b.b,
                b.c.a, b.c.b
            );
        }
    }
}
