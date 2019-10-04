using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class OutOfMemoryException : ByteCodeExecutionException
    {
        public OutOfMemoryException() { }
        public OutOfMemoryException(string message) : base(message) { }
        public OutOfMemoryException(string message, System.Exception inner) : base(message, inner) { }
        protected OutOfMemoryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
