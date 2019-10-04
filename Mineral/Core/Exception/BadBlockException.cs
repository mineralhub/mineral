using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class BadBlockException : System.Exception
    {
        public BadBlockException() { }
        public BadBlockException(string message) : base(message) { }
        public BadBlockException(string message, System.Exception inner) : base(message, inner) { }
        protected BadBlockException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
