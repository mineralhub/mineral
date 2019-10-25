using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class InvalidTypeException : System.Exception
    {
        public InvalidTypeException() { }
        public InvalidTypeException(string message) : base(message) { }
        public InvalidTypeException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
