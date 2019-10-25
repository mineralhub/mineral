using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ConfigrationException : System.Exception
    {
        public ConfigrationException() { }
        public ConfigrationException(string message) : base(message) { }
        public ConfigrationException(string message, System.Exception inner) : base(message, inner) { }
        protected ConfigrationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
