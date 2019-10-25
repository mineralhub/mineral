using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class VMIllegalException : System.Exception
    {
        public VMIllegalException() { }
        public VMIllegalException(string message) : base(message) { }
        public VMIllegalException(string message, System.Exception inner) : base(message, inner) { }
        protected VMIllegalException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
