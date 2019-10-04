using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class StackTooSmallException : ByteCodeExecutionException
    {
        public StackTooSmallException() { }
        public StackTooSmallException(string message) : base(message) { }
        public StackTooSmallException(string message, System.Exception inner) : base(message, inner) { }
        public StackTooSmallException(int expected_size, int actual_size)
            : this(string.Format("Expected stack size {0} but actual %d", expected_size, actual_size))
        {
        }
        protected StackTooSmallException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
