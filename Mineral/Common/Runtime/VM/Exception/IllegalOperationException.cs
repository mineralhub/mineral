using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class IllegalOperationException : ByteCodeExecutionException
    {
        public IllegalOperationException() { }
        public IllegalOperationException(string message) : base(message) { }
        public IllegalOperationException(string message, System.Exception inner) : base(message, inner) { }
        protected IllegalOperationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
