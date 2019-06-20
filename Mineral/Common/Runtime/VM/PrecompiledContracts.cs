using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core;
using Mineral.Cryptography;
using Mineral.Utils;

namespace Mineral.Common.Runtime.VM
{
    public partial class PrecompiledContracts
    {
        #region Field
        private static readonly ECRecover ecrecover = new ECRecover();
        private static readonly SHA256 sha256 = new SHA256();
        private static readonly Ripempd160 ripempd160 = new Ripempd160();
        private static readonly Identity identity = new Identity();
        private static readonly ModExp modexp = new ModExp();
        private static readonly BN128Addition addition = new BN128Addition();
        private static readonly BN128Multiplication multiplication = new BN128Multiplication();
        private static readonly BN128Pairing pairing = new BN128Pairing();

        private static readonly DataWord ecrecover_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000001");
        private static readonly DataWord sha256_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000002");
        private static readonly DataWord ripempd160_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000003");
        private static readonly DataWord identity_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000004");
        private static readonly DataWord modexp_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000005");
        private static readonly DataWord addition_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000006");
        private static readonly DataWord multiplication_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000007");
        private static readonly DataWord pairing_address =
            new DataWord("0000000000000000000000000000000000000000000000000000000000000008");
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static byte[] EncodeRes(byte[] w1, byte[] w2)
        {
            byte[] res = new byte[64];

            w1 = ByteUtil.StripLeadingZeroes(w1);
            w2 = ByteUtil.StripLeadingZeroes(w2);

            Array.Copy(w1, 0, res, 32 - w1.Length, w1.Length);
            Array.Copy(w2, 0, res, 64 - w2.Length, w2.Length);

            return res;
        }
        #endregion


        #region External Method
        public static PrecompiledContract getContractForAddress(DataWord address)
        {
            if (address == null)
            {
                return identity;
            }
            if (address.Equals(ecrecover_address))
            {
                return ecrecover;
            }
            if (address.Equals(sha256_address))
            {
                return sha256;
            }
            if (address.Equals(ripempd160_address))
            {
                return ripempd160;
            }
            if (address.Equals(identity_address))
            {
                return identity;
            }
            if (address.Equals(modexp_address))
            {
                return modexp;
            }
            if (address.Equals(addition_address))
            {
                return addition;
            }
            if (address.Equals(multiplication_address))
            {
                return multiplication;
            }
            if (address.Equals(pairing_address))
            {
                return pairing;
            }
            return null;
        }
        #endregion
    }
}
