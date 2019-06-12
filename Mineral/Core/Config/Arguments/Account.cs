using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Converter;
using Mineral.Utils;
using Newtonsoft.Json;

namespace Mineral.Core.Config.Arguments
{
    public class Account
    {
        public enum AccountType
        {
            Normal,
            AssetIssue,
            Contract,
        }

        #region Field
        #endregion


        #region Property
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        [JsonConverter(typeof(JsonArgAccountTypeConverter))]
        public AccountType Type { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("balance")]
        public long Balance { get; set; }
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
