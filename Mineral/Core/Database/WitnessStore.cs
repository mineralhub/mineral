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
        public List<WitnessCapsule> AllWitnesses
        {
            get
            {
                List<WitnessCapsule> result = new List<WitnessCapsule>();
                IEnumerator<KeyValuePair<byte[], WitnessCapsule>> it = GetEnumerator();
                while (it.MoveNext())
                {
                    result.Add(it.Current.Value);
                }

                return result;
            }
        }

        #endregion


        #region Contructor
        public WitnessStore(IRevokingDatabase revoking_database, string db_name = "witness")
            : base(revoking_database, db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override WitnessCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);
            return value.IsNotNullOrEmpty() ? new WitnessCapsule(value) : null;
        }
        #endregion
    }
}
