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
        [JsonConverter(typeof(JsonUInt160Converter))]
        public UInt160 Address { get; set; }
        [JsonProperty("balance")]
        [JsonConverter(typeof(JsonFixed8Converter))]
        public Fixed8 Balance { get; set; }
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
