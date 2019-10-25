using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.LogsFilter.Capsule;
using Mineral.Common.Runtime.VM;

namespace Mineral.Common.LogsFilter.Trigger
{
    public class ContractTrigger : Trigger
    {
        #region Field
        #endregion


        #region Property
        public string UniqueId { get; set; }
        public string TransactionId { get; set; }
        public string ContractAddress { get; set; }
        public string CallerAddress { get; set; }
        public string OriginAddress { get; set; }
        public string CreatorAddress { get; set; }
        public long BlockNumber { get; set; }
        public bool IsRemoved { get; set; }
        public long LatestSolidifiedBlockNumber { get; set; }
        public LogInfo LogInfo { get; set; }
        public RawData Raw { get; set; }
        public string AbiString { get; set; }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
