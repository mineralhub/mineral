using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class PrecompiledContractException : ByteCodeExecutionException
    {
        public PrecompiledContractException() { }
        public PrecompiledContractException(string message) : base(message) { }
        public PrecompiledContractException(string message, System.Exception inner) : base(message, inner) { }
        protected PrecompiledContractException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
