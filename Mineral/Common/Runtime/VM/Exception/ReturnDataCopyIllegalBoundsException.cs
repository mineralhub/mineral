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
        public ReturnDataCopyIllegalBoundsException(DataWord offset, DataWord size, long return_data_size)
                        : base(string.Format("Illegal RETURNDATACOPY arguments: offset (%s) + size (%s) > RETURNDATASIZE (%d)",
                                             offset,
                                             size,
                                             return_data_size))
        {
        }
        protected ReturnDataCopyIllegalBoundsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
