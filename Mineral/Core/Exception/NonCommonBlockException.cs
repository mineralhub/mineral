using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class NonCommonBlockException : System.Exception
    {
        public NonCommonBlockException() { }
        public NonCommonBlockException(string message) : base(message) { }
        public NonCommonBlockException(string message, System.Exception inner) : base(message, inner) { }
        protected NonCommonBlockException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
