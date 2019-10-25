using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class BadNumberBlockException : System.Exception
    {
        public BadNumberBlockException() { }
        public BadNumberBlockException(string message) : base(message) { }
        public BadNumberBlockException(string message, System.Exception inner) : base(message, inner) { }
        protected BadNumberBlockException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
