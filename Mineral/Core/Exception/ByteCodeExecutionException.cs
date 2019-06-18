using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ByteCodeExecutionException : System.Exception
    {
        public ByteCodeExecutionException() { }
        public ByteCodeExecutionException(string message) : base(message) { }
        public ByteCodeExecutionException(string message, System.Exception inner) : base(message, inner) { }
        protected ByteCodeExecutionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
