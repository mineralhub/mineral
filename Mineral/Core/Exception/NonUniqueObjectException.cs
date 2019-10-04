using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class NonUniqueObjectException : System.Exception
    {
        public NonUniqueObjectException() { }
        public NonUniqueObjectException(string message) : base(message) { }
        public NonUniqueObjectException(string message, System.Exception inner) : base(message, inner) { }
        protected NonUniqueObjectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
