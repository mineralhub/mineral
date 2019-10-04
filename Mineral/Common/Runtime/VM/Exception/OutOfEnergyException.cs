using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class OutOfEnergyException : ByteCodeExecutionException
    {
        public OutOfEnergyException() { }
        public OutOfEnergyException(string message) : base(message) { }
        public OutOfEnergyException(string message, System.Exception inner) : base(message, inner) { }
        protected OutOfEnergyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
