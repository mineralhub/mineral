using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class BadItemException : System.Exception
    {
        public BadItemException() { }
        public BadItemException(string message) : base(message) { }
        public BadItemException(string message, System.Exception inner) : base(message, inner) { }
        protected BadItemException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
