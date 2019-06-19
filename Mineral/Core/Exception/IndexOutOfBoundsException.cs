using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class IndexOutOfBoundsException : System.Exception
    {
        public IndexOutOfBoundsException() { }
        public IndexOutOfBoundsException(string message) : base(message) { }
        public IndexOutOfBoundsException(string message, System.Exception inner) : base(message, inner) { }
        protected IndexOutOfBoundsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
