using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class OutOfStorageException : ByteCodeExecutionException
    {
        public OutOfStorageException() { }
        public OutOfStorageException(string message) : base(message) { }
        public OutOfStorageException(string message, System.Exception inner) : base(message, inner) { }
        protected OutOfStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
