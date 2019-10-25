using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class VMStackOverFlowException : ByteCodeExecutionException
    {
        public VMStackOverFlowException() { }
        public VMStackOverFlowException(string message) : base(message) { }
        public VMStackOverFlowException(string message, System.Exception inner) : base(message, inner) { }
        protected VMStackOverFlowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
