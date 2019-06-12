using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Utils;

namespace Mineral.Core.Database
{
    public class WitnessStore : MineralStoreWithRevoking<WitnessCapsule, Protocol.Witness>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public WitnessStore(string db_name = "witness") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public List<WitnessCapsule> GetAllWitnesses()
        {
            List<WitnessCapsule> result = new List<WitnessCapsule>();
            IEnumerator<KeyValuePair<byte[], WitnessCapsule>> it = GetEnumerator();
            while (it.MoveNext())
            {
                result.Add(it.Current.Value);
            }

            return result;
        }

        public WitnessCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);
            return value.IsNotNullOrEmpty() ? new WitnessCapsule(value) : null;
        }
        #endregion
    }
}
