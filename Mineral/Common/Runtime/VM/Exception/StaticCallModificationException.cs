using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class StaticCallModificationException : ByteCodeExecutionException
    {
        public StaticCallModificationException() { }
        public StaticCallModificationException(string message) : base(message) { }
        public StaticCallModificationException(string message, System.Exception inner) : base(message, inner) { }
        protected StaticCallModificationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
