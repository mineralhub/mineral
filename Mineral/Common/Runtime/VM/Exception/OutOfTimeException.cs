using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class OutOfTimeException : ByteCodeExecutionException
    {
        public OutOfTimeException() { }
        public OutOfTimeException(string message) : base(message) { }
        public OutOfTimeException(string message, System.Exception inner) : base(message, inner) { }
        protected OutOfTimeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
