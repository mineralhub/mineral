using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Mineral.Common.Runtime.VM
{
    public class AbiEntity
    {
        public class Component
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("type")]
            public string ComponentType { get; set; }
        }

        public class InOut
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("type")]
            public string InOutType { get; set; }
            [JsonProperty("indexed")]
            public bool Indexed { get; set; }
            [JsonProperty("components")]
            public List<Component> Components { get; set; }
        }

        [JsonProperty("anonymous")]
        public bool Anonymous { get; set; }
        [JsonProperty("constant")]
        public bool Constant { get; set; }
        [JsonProperty("payable")]
        public bool Payable { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string AbiType { get; set; }
        [JsonProperty("stateMutability")]
        public string StateMutability { get; set; }
        [JsonProperty("inputs")]
        public List<InOut> Inputs { get; set; }
        [JsonProperty("outputs")]
        public List<InOut> Outputs { get; set; }
    }
}
