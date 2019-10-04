using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class MemoryOverflowException : System.Exception
    {
        public MemoryOverflowException() { }
        public MemoryOverflowException(string message) : base(message) { }
        public MemoryOverflowException(string message, System.Exception inner) : base(message, inner) { }
        public MemoryOverflowException(OpCode code)
            : base(string.Format("Out of Memory when '{0}' operation executing", code.ToString()))
        {
        }
        protected MemoryOverflowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
