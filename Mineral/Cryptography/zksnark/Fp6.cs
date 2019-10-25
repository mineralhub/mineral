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
 * Arithmetic in Fp_6 <br/> <br/>
 *
 * "p" equals 21888242871839275222246405745257275088696311157297823662689037894645226208583, <br/>
 * elements of Fp_6 are represented with 3 elements of {@link Fp2} <br/> <br/>
 *
 * Field arithmetic is ported from <a href="https://github.com/scipr-lab/libff/blob/master/libff/algebra/fields/fp6_3over2.tcc">libff</a>
 *
 * @author Mikhail Kalinin
 * @since 05.09.2017
 */

namespace Mineral.Cryptography.zksnark
{
    public class Fp6 : IField<Fp6>
    {
        public static readonly Fp6 ZERO = new Fp6(Fp2.ZERO, Fp2.ZERO, Fp2.ZERO);
        public static readonly Fp6 _1 = new Fp6(Fp2._1, Fp2.ZERO, Fp2.ZERO);
        public static readonly Fp2 NON_RESIDUE = new Fp2(BigInteger.ValueOf(9), BigInteger.One);

        static readonly Fp2[] FROBENIUS_COEFFS_B = {
                                    new Fp2(BigInteger.One,
                                        BigInteger.Zero),

                                    new Fp2(new BigInteger("21575463638280843010398324269430826099269044274347216827212613867836435027261"),
                                        new BigInteger("10307601595873709700152284273816112264069230130616436755625194854815875713954")),

                                    new Fp2(new BigInteger("21888242871839275220042445260109153167277707414472061641714758635765020556616"),
                                        BigInteger.Zero),

                                    new Fp2(new BigInteger("3772000881919853776433695186713858239009073593817195771773381919316419345261"),
                                        new BigInteger("2236595495967245188281701248203181795121068902605861227855261137820944008926")),

                                    new Fp2(new BigInteger("2203960485148121921418603742825762020974279258880205651966"),
                                        BigInteger.Zero),

                                    new Fp2(new BigInteger("18429021223477853657660792034369865839114504446431234726392080002137598044644"),
                                        new BigInteger("9344045779998320333812420223237981029506012124075525679208581902008406485703"))
        };

        static readonly Fp2[] FROBENIUS_COEFFS_C = {
                                  new Fp2(BigInteger.One,
                                      BigInteger.Zero),

                                  new Fp2(new BigInteger("2581911344467009335267311115468803099551665605076196740867805258568234346338"),
                                      new BigInteger("19937756971775647987995932169929341994314640652964949448313374472400716661030")),

                                  new Fp2(new BigInteger("2203960485148121921418603742825762020974279258880205651966"),
                                      BigInteger.Zero),

                                  new Fp2(new BigInteger("5324479202449903542726783395506214481928257762400643279780343368557297135718"),
                                      new BigInteger("16208900380737693084919495127334387981393726419856888799917914180988844123039")),

                                  new Fp2(new BigInteger("21888242871839275220042445260109153167277707414472061641714758635765020556616"),
                                      BigInteger.Zero),

                                  new Fp2(new BigInteger("13981852324922362344252311234282257507216387789820983642040889267519694726527"),
                                      new BigInteger("7629828391165209371577384193250820201684255241773809077146787135900891633097"))
        };

        public Fp2 a;
        public Fp2 b;
        public Fp2 c;

        public Fp6(Fp2 a, Fp2 b, Fp2 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Fp6 Squared()
        {
            Fp2 s0 = a.Squared();
            Fp2 ab = a.Mul(b);
            Fp2 s1 = ab.Dbl();
            Fp2 s2 = a.Sub(b).Add(c).Squared();
            Fp2 bc = b.Mul(c);
            Fp2 s3 = bc.Dbl();
            Fp2 s4 = c.Squared();

            Fp2 ra = s0.Add(s3.MulByNonResidue());
            Fp2 rb = s1.Add(s4.MulByNonResidue());
            Fp2 rc = s1.Add(s2).Add(s3).Sub(s0).Sub(s4);

            return new Fp6(ra, rb, rc);
        }

        public Fp6 Dbl()
        {
            return this.Add(this);
        }

        public Fp6 Mul(Fp6 o)
        {

            Fp2 a1 = a, b1 = b, c1 = c;
            Fp2 a2 = o.a, b2 = o.b, c2 = o.c;

            Fp2 a1a2 = a1.Mul(a2);
            Fp2 b1b2 = b1.Mul(b2);
            Fp2 c1c2 = c1.Mul(c2);

            Fp2 ra = a1a2.Add(b1.Add(c1).Mul(b2.Add(c2)).Sub(b1b2).Sub(c1c2).MulByNonResidue());
            Fp2 rb = a1.Add(b1).Mul(a2.Add(b2)).Sub(a1a2).Sub(b1b2).Add(c1c2.MulByNonResidue());
            Fp2 rc = a1.Add(c1).Mul(a2.Add(c2)).Sub(a1a2).Add(b1b2).Sub(c1c2);

            return new Fp6(ra, rb, rc);
        }

        public Fp6 Mul(Fp2 o)
        {

            Fp2 ra = a.Mul(o);
            Fp2 rb = b.Mul(o);
            Fp2 rc = c.Mul(o);

            return new Fp6(ra, rb, rc);
        }

        public Fp6 MulByNonResidue()
        {
            Fp2 ra = NON_RESIDUE.Mul(c);
            Fp2 rb = a;
            Fp2 rc = b;

            return new Fp6(ra, rb, rc);
        }

        public Fp6 Add(Fp6 o)
        {
            Fp2 ra = a.Add(o.a);
            Fp2 rb = b.Add(o.b);
            Fp2 rc = c.Add(o.c);

            return new Fp6(ra, rb, rc);
        }

        public Fp6 Sub(Fp6 o)
        {
            Fp2 ra = a.Sub(o.a);
            Fp2 rb = b.Sub(o.b);
            Fp2 rc = c.Sub(o.c);

            return new Fp6(ra, rb, rc);
        }

        public Fp6 Inverse()
        {
            /* From "High-Speed Software Implementation of the Optimal Ate Pairing over Barreto-Naehrig Curves"; Algorithm 17 */
            Fp2 t0 = a.Squared();
            Fp2 t1 = b.Squared();
            Fp2 t2 = c.Squared();
            Fp2 t3 = a.Mul(b);
            Fp2 t4 = a.Mul(c);
            Fp2 t5 = b.Mul(c);
            Fp2 c0 = t0.Sub(t5.MulByNonResidue());
            Fp2 c1 = t2.MulByNonResidue().Sub(t3);
            Fp2 c2 = t1.Sub(t4); // typo in paper referenced above. should be "-" as per Scott, but is "*"
            Fp2 t6 = a.Mul(c0).Add((c.Mul(c1).Add(b.Mul(c2))).MulByNonResidue()).Inverse();

            Fp2 ra = t6.Mul(c0);
            Fp2 rb = t6.Mul(c1);
            Fp2 rc = t6.Mul(c2);

            return new Fp6(ra, rb, rc);
        }

        public Fp6 Negate()
        {
            return new Fp6(a.Negate(), b.Negate(), c.Negate());
        }

        public bool IsZero()
        {
            return this.Equals(ZERO);
        }


        public bool IsValid()
        {
            return a.IsValid() && b.IsValid() && c.IsValid();
        }

        public Fp6 FrobeniusMap(int power)
        {
            Fp2 ra = a.FrobeniusMap(power);
            Fp2 rb = FROBENIUS_COEFFS_B[power % 6].Mul(b.FrobeniusMap(power));
            Fp2 rc = FROBENIUS_COEFFS_C[power % 6].Mul(c.FrobeniusMap(power));

            return new Fp6(ra, rb, rc);
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

            Fp6 fp6 = (Fp6)o;

            if (a != null ? !a.Equals(fp6.a) : fp6.a != null)
            {
                return false;
            }
            if (b != null ? !b.Equals(fp6.b) : fp6.b != null)
            {
                return false;
            }
            return !(c != null ? !c.Equals(fp6.c) : fp6.c != null);
        }

        public override int GetHashCode()
        {
            return (a.GetHashCode() + b.GetHashCode() + c.GetHashCode()).GetHashCode();
        }
    }   
}
