using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class StackTooLargeException : System.Exception
    {
        public StackTooLargeException() { }
        public StackTooLargeException(string message) : base(message) { }
        public StackTooLargeException(string message, System.Exception inner) : base(message, inner) { }
        protected StackTooLargeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
