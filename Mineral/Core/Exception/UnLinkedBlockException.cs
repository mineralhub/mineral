using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class UnLinkedBlockException : System.Exception
    {
        public UnLinkedBlockException() { }
        public UnLinkedBlockException(string message) : base(message) { }
        public UnLinkedBlockException(string message, System.Exception inner) : base(message, inner) { }
        protected UnLinkedBlockException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
