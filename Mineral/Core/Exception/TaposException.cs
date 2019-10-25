using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class TaposException : System.Exception
    {
        public TaposException() { }
        public TaposException(string message) : base(message) { }
        public TaposException(string message, System.Exception inner) : base(message, inner) { }
        protected TaposException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
