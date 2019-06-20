using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Common.Utils;
using Mineral.Cryptography;
using Mineral.Cryptography.zksnark;
using Mineral.Utils;

namespace Mineral.Common.Runtime.VM
{
    public partial class PrecompiledContracts
    {
        public abstract class PrecompiledContract
        {
            public byte[] CallerAddress { get; set; }
            public ProgramResult Result { get; set; }
            public IDeposit Despoit { get; set; }
            public bool IsStaticCall { get; set; }

            public abstract long GetEnergyForData(byte[] data);
            public abstract KeyValuePair<bool, byte[]> Execute(byte[] data);
        }

        public class Identity : PrecompiledContract
        {
            public override long GetEnergyForData(byte[] data)
            {
                if (data == null)
                {
                    return 15;
                }
                return 15L + (data.Length + 31) / 32 * 3;
            }

            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                return new KeyValuePair<bool, byte[]>(true, data);
            }
        }

        public class SHA256 : PrecompiledContract
        {
            public override long GetEnergyForData(byte[] data)
            {
                if (data == null)
                {
                    return 60;
                }

                return 60L + (data.Length + 31) / 32 * 12;
            }

            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                if (data == null)
                {
                    return new KeyValuePair<bool, byte[]>(true, SHA256Hash.ToHash(new byte[0]));
                }

                return new KeyValuePair<bool, byte[]>(true, SHA256Hash.ToHash(data));
            }
        }


        public class Ripempd160 : PrecompiledContract
        {
            public override long GetEnergyForData(byte[] data)
            {
                if (data == null)
                {
                    return 600;
                }
                return 600L + (data.Length + 31) / 32 * 120;
            }


            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                byte[] target = new byte[20];
                if (data == null)
                {
                    data = new byte[0];
                }
                byte[] orig = SHA256Hash.ToHash(data);
                Array.Copy(orig, 0, target, 0, 20);
                return new KeyValuePair<bool, byte[]>(true, SHA256Hash.ToHash(target));
            }
        }

        public class ECRecover : PrecompiledContract
        {
            private static bool ValidateV(byte[] v)
            {
                for (int i = 0; i < v.Length - 1; i++)
                {
                    if (v[i] != 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            public override long GetEnergyForData(byte[] data)
            {
                return 3000;
            }

            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                byte[] h = new byte[32];
                byte[] v = new byte[32];
                byte[] r = new byte[32];
                byte[] s = new byte[32];

                DataWord out_val = null;

                try
                {
                    Array.Copy(data, 0, h, 0, 32);
                    Array.Copy(data, 32, v, 0, 32);
                    Array.Copy(data, 64, r, 0, 32);

                    int length = data.Length < 128 ? data.Length - 96 : 32;
                    Array.Copy(data, 96, s, 0, length);

                    ECDSASignature signature = ECDSASignatureFactory.FromComponents(r, s, v[31]);
                    if (ValidateV(v) && signature.ValidateComponents())
                    {
                        out_val = new DataWord(ECKey.SignatureToAddress(h, signature));
                    }
                }
                catch
                {
                }

                if (out_val == null) {
                    return new KeyValuePair<bool, byte[]>(true, new byte[0]);
                } else {
                    return new KeyValuePair<bool, byte[]>(true, out_val.Data);
                }
            }
        }

        public class ModExp : PrecompiledContract
        {
            private static readonly BigInteger GQUAD_DIVISOR = new BigInteger(20);
            private static readonly int ARGS_OFFSET = 32 * 3;

            private long GetMultComplexity(long x)
            {
                long x2 = x * x;

                if (x <= 64)
                {
                    return x2;
                }
                if (x <= 1024)
                {
                    return x2 / 4 + 96 * x - 3072;
                }

                return x2 / 16 + 480 * x - 199680;
            }

            private long GetAdjustedExponentLength(byte[] exp_high, long exp_len)
            {
                int leading_zeros = ByteUtil.NumberOfLeadingZeros(exp_high);
                int highest_bit = 8 * exp_high.Length - leading_zeros;

                if (highest_bit > 0)
                {
                    highest_bit--;
                }

                if (exp_len <= 32)
                {
                    return highest_bit;
                }
                else
                {
                    return 8 * (exp_len - 32) + highest_bit;
                }
            }

            private int ParseLen(byte[] data, int idx)
            {
                byte[] bytes = ByteUtil.ParseBytes(data, 32 * idx, 32);
                return new DataWord(bytes).ToIntSafety();
            }

            private BigInteger ParseArg(byte[] data, int offset, int len)
            {
                byte[] bytes = ByteUtil.ParseBytes(data, offset, len);
                return ByteUtil.BytesToBigInteger(bytes);
            }

            public override long GetEnergyForData(byte[] data)
            {
                if (data == null)
                    data = new byte[0];

                int base_len = ParseLen(data, 0);
                int exp_len = ParseLen(data, 1);
                int mod_len = ParseLen(data, 2);

                byte[] exp_high = ByteUtil.ParseBytes(data, (int)BigInteger.Add(ARGS_OFFSET, base_len), Math.Min(exp_len, 32));

                long mult_complexity = GetMultComplexity(Math.Max(base_len, mod_len));
                long adjust_exp_len = GetAdjustedExponentLength(exp_high, exp_len);

                // use big numbers to stay safe in case of overflow
                BigInteger energy = new BigInteger(mult_complexity);
                energy = BigInteger.Multiply(energy, new BigInteger(Math.Max(adjust_exp_len, 1)));
                energy = BigInteger.Divide(energy, GQUAD_DIVISOR);

                return energy.CompareTo(new BigInteger(long.MaxValue)) <0 ? (long)energy : long.MaxValue;
            }

            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                if (data == null)
                {
                    return new KeyValuePair<bool, byte[]>(true, new byte[0]);
                }

                int base_len = ParseLen(data, 0);
                int exp_len = ParseLen(data, 1);
                int mod_len = ParseLen(data, 2);

                BigInteger base_val = ParseArg(data, ARGS_OFFSET, base_len);
                BigInteger exp_val = ParseArg(data, (int)BigInteger.Add(ARGS_OFFSET, base_len), exp_len);
                BigInteger mod_val = ParseArg(data, (int)BigInteger.Add(BigInteger.Add(ARGS_OFFSET, base_len), exp_len), mod_len);

                if (mod_val.CompareTo(BigInteger.Zero) == 0)
                    return new KeyValuePair<bool, byte[]>(true, new byte[0]);

                byte[] result = ByteUtil.StripLeadingZeroes(BigInteger.ModPow(base_val, exp_val, mod_val).ToByteArray());
                if (result.Length < mod_len)
                {

                    byte[] adjust_result = new byte[mod_len];
                    Array.Copy(result, 0, adjust_result, mod_len - result.Length, result.Length);

                    return new KeyValuePair<bool, byte[]>(true, adjust_result);
                }
                else
                {
                    return new KeyValuePair<bool, byte[]>(true, result);
                }
            }
        }

        public class BN128Addition : PrecompiledContract
        {
            public override long GetEnergyForData(byte[] data)
            {
                return 500;
            }
            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                if (data == null)
                {
                    data = new byte[0];
                }

                byte[] x1 = ByteUtil.ParseWord(data, 0);
                byte[] y1 = ByteUtil.ParseWord(data, 1);

                byte[] x2 = ByteUtil.ParseWord(data, 2);
                byte[] y2 = ByteUtil.ParseWord(data, 3);

                BN128<Fp> p1 = BN128Fp.Create(x1, y1);
                if (p1 == null)
                {
                    return new KeyValuePair<bool, byte[]>(false, new byte[0]);
                }

                BN128<Fp> p2 = BN128Fp.Create(x2, y2);
                if (p2 == null)
                {
                    return new KeyValuePair<bool, byte[]>(false, new byte[0]);
                }

                BN128<Fp> res = p1.Add(p2).ToEthNotation();

                return new KeyValuePair<bool, byte[]>(true, EncodeRes(res.x.Bytes(), res.y.Bytes()));
            }
        }

        public class BN128Multiplication : PrecompiledContract
        {
            public override long GetEnergyForData(byte[] data)
            {
                return 40000;
            }

            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                if (data == null)
                {
                    data = new byte[0];
                }

                byte[] x = ByteUtil.ParseWord(data, 0);
                byte[] y = ByteUtil.ParseWord(data, 1);

                byte[] s = ByteUtil.ParseWord(data, 2);

                BN128<Fp> p = BN128Fp.Create(x, y);
                if (p == null)
                {
                    return new KeyValuePair<bool, byte[]>(false, new byte[0]);
                }

                BN128<Fp> res = p.Mul(new Org.BouncyCastle.Math.BigInteger(1, s)).ToEthNotation();

                return new KeyValuePair<bool, byte[]>(true, EncodeRes(res.x.Bytes(), res.y.Bytes()));
            }
        }

        public class BN128Pairing : PrecompiledContract
        {
            private static readonly int PAIR_SIZE = 192;

            public override long GetEnergyForData(byte[] data)
            {
                if (data == null)
                {
                    return 100000;
                }

                return 80000L * (data.Length / PAIR_SIZE) + 100000;
            }

            public override KeyValuePair<bool, byte[]> Execute(byte[] data)
            {
                if (data == null)
                {
                    data = new byte[0];
                }

                if (data.Length % PAIR_SIZE > 0)
                {
                    return new KeyValuePair<bool, byte[]>(false, new byte[0]);
                }

                PairingCheck check = PairingCheck.Create();

                // iterating over all pairs
                for (int offset = 0; offset < data.Length; offset += PAIR_SIZE)
                {
                    KeyValuePair<BN128G1, BN128G2> pair = DecodePair(data, offset);

                    if (pair.Equals(default(KeyValuePair<BN128G1, BN128G2>)))
                    {
                        return new KeyValuePair<bool, byte[]>(false, new byte[0]);
                    }

                    check.AddPair(pair.Key, pair.Value);
                }

                check.Run();
                int result = check.Result();

                return new KeyValuePair<bool, byte[]>(true, new DataWord(result).Data);
            }

            private KeyValuePair<BN128G1, BN128G2> DecodePair(byte[] input, int offset)
            {
                byte[] x = ByteUtil.ParseWord(input, offset, 0);
                byte[] y = ByteUtil.ParseWord(input, offset, 1);

                BN128G1 p1 = BN128G1.Create(x, y);

                // fail if point is invalid
                if (p1 == null)
                {
                    return default(KeyValuePair<BN128G1, BN128G2>);
                }

                // (b, a)
                byte[] b = ByteUtil.ParseWord(input, offset, 2);
                byte[] a = ByteUtil.ParseWord(input, offset, 3);

                // (d, c)
                byte[] d = ByteUtil.ParseWord(input, offset, 4);
                byte[] c = ByteUtil.ParseWord(input, offset, 5);

                BN128G2 p2 = BN128G2.Create(a, b, c, d);

                // fail if point is invalid
                if (p2 == null)
                {
                    return default(KeyValuePair<BN128G1, BN128G2>);
                }

                return new KeyValuePair<BN128G1, BN128G2>(p1, p2);
            }
        }
    }
}
