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
* Arithmetic in F_p, p = 21888242871839275222246405745257275088696311157297823662689037894645226208583
*
* @author Mikhail Kalinin
* @since 01.09.2017
*/
namespace Mineral.Cryptography.zksnark
{
    public class Fp : IField<Fp>
    {
        public static readonly Fp ZERO = new Fp(BigInteger.Zero);
        public static readonly Fp _1 = new Fp(BigInteger.One);
        public static readonly Fp NON_RESIDUE = new Fp(new BigInteger("21888242871839275222246405745257275088696311157297823662689037894645226208582"));
        public static readonly Fp _2_INV = new Fp(BigInteger.ValueOf(2).ModInverse(Parameters.P));

        private BigInteger v;

        public Fp(BigInteger v)
        {
            this.v = v;
        }

        public Fp Add(Fp o)
        {
            return new Fp(this.v.Add(o.v).Mod(Parameters.P));
        }

        public Fp Mul(Fp o)
        {
            return new Fp(this.v.Multiply(o.v).Mod(Parameters.P));
        }

        public Fp Sub(Fp o)
        {
            return new Fp(this.v.Subtract(o.v).Mod(Parameters.P));
        }

        public Fp Squared()
        {
            return new Fp(v.Multiply(v).Mod(Parameters.P));
        }

        public Fp Dbl()
        {
            return new Fp(v.Add(v).Mod(Parameters.P));
        }

        public Fp Inverse()
        {
            return new Fp(v.ModInverse(Parameters.P));
        }

        public Fp Negate()
        {
            return new Fp(v.Negate().Mod(Parameters.P));
        }

        public bool IsZero()
        {
            return v.CompareTo(BigInteger.Zero) == 0;
        }

        /**
         * Checks if provided value is a valid Fp member
         */
        public bool IsValid()
        {
            return v.CompareTo(Parameters.P) < 0;
        }

        public Fp2 Mul(Fp2 o)
        {
            return new Fp2(o.a.Mul(this), o.b.Mul(this));
        }

        public static Fp Create(byte[] v)
        {
            return new Fp(new BigInteger(1, v));
        }

        public static Fp Create(BigInteger v)
        {
            return new Fp(v);
        }

        public byte[] Bytes()
        {
            return v.ToByteArray();
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

            Fp fp = (Fp)o;

            return !(v != null ? v.CompareTo(fp.v) != 0 : fp.v != null);
        }

        public override int GetHashCode()
        {
            return v.GetHashCode();
        }

        public override string ToString()
        {
            return v.ToString();
        }
    }

}
