using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class WitnessScheduleStore : MineralStoreWithRevoking<BytesCapsule, object>
    {
        #region Field
        private static readonly byte[] ACTIVE_WITNESSES = Encoding.UTF8.GetBytes("active_witnesses");
        private static readonly byte[] CURRENT_SHUFFLED_WITNESSES = Encoding.UTF8.GetBytes("current_shuffled_witnesses");
        private static readonly int ADDRESS_BYTE_ARRAY_LENGTH = 21;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public WitnessScheduleStore(string db_name = "witness_schedule") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void SaveData(byte[] species, List<ByteString> witness_address)
        {
            int i = 0;
            byte[] data = new byte[witness_address.Count * ADDRESS_BYTE_ARRAY_LENGTH];
            foreach (ByteString address in witness_address)
            {
                Array.Copy(address.ToByteArray(), 0, data, i * ADDRESS_BYTE_ARRAY_LENGTH, ADDRESS_BYTE_ARRAY_LENGTH);
                i++;
            }
            Put(species, new BytesCapsule(data));
        }

        public List<ByteString> GetData(byte[] species)
        {
            List<ByteString> witness_address = new List<ByteString>();
            BytesCapsule data = GetUnchecked(species);

            int length = data.Data.Length / ADDRESS_BYTE_ARRAY_LENGTH;
            for (int i = 0; i < length; i++)
            {
                byte[] b = new byte[ADDRESS_BYTE_ARRAY_LENGTH];
                Array.Copy(data.Data, i * ADDRESS_BYTE_ARRAY_LENGTH, b, 0, ADDRESS_BYTE_ARRAY_LENGTH);
                witness_address.Add(ByteString.CopyFrom(b));
            }

            return witness_address;
        }

        public void SaveActiveWitnesses(List<ByteString> witness_address)
        {
            SaveData(ACTIVE_WITNESSES, witness_address);
        }

        public List<ByteString> GetActiveWitnesses()
        {
            return GetData(ACTIVE_WITNESSES);
        }

        public void SaveCurrentShuffledWitnesses(List<ByteString> witness_address)
        {
            SaveData(CURRENT_SHUFFLED_WITNESSES, witness_address);
        }

        public List<ByteString> GetCurrentShuffledWitnesses()
        {
            return GetData(CURRENT_SHUFFLED_WITNESSES);
        }
        #endregion
    }
}
