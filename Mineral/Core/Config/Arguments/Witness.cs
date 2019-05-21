using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Converter;
using Mineral.Utils;
using Newtonsoft.Json;

namespace Mineral.Core.Config.Arguments
{
    public class Witness
    {
        #region Field
        #endregion


        #region Property
        [JsonProperty("address")]
        [JsonConverter(typeof(JsonUInt160Converter))]
        public UInt160 Address { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("vote_count")]
        public Fixed8 VoteCount { get; set; }
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
