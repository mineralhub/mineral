using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM
{
    public class CallCreate
    {
        #region Field
        private readonly byte[] data = null;
        private readonly byte[] destination = null;
        private readonly byte[] energy_limit = null;
        private readonly byte[] value = null;
        #endregion


        #region Property
        public byte[] Data
        {
            get { return this.data; }
        }

        public byte[] Destination
        {
            get { return this.destination; }
        }

        public byte[] EnergyLimit
        {
            get { return this.energy_limit; }
        }

        public byte[] Value
        {
            get { return this.value; }
        }
        #endregion


        #region Constructor
        public CallCreate(byte[] data, byte[] destination, byte[] energy_limit, byte[] value)
        {
            this.data = data;
            this.destination = destination;
            this.energy_limit = energy_limit;
            this.value = value;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
