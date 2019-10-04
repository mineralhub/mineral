using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM;

namespace Mineral.Common.LogsFilter.Capsule
{
    public class RawData
    {
        #region Field
        private string address = "";
        private string data = "";
        private List<DataWord> topics = null;
        #endregion


        #region Property
        public string Address
        {
            get { return this.address; }
        }

        public string Data
        {
            get { return this.data; }
        }

        public List<DataWord> Topics
        {
            get { return this.topics; }
        }
        #endregion


        #region Constructor
        public RawData(byte[] address, List<DataWord> topics, byte[] data)
        {
            this.address = address != null ? address.ToHexString() : "";
            this.data = data != null ? data.ToHexString() : "";
            this.topics = topics ?? new List<DataWord>();
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
