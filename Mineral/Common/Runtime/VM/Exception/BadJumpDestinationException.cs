using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class BadJumpDestinationException : ByteCodeExecutionException
    {
        public BadJumpDestinationException() { }
        public BadJumpDestinationException(string message) : base(message) { }
        public BadJumpDestinationException(string message, System.Exception inner) : base(message, inner) { }
        protected BadJumpDestinationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
