using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class ReturnDataCopyIllegalBoundsException : ByteCodeExecutionException
    {
        public ReturnDataCopyIllegalBoundsException() { }
        public ReturnDataCopyIllegalBoundsException(string message) : base(message) { }
        public ReturnDataCopyIllegalBoundsException(string message, System.Exception inner) : base(message, inner) { }
        protected ReturnDataCopyIllegalBoundsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
