using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;

namespace Mineral.Common.Runtime.VM.Exception
{
    [Serializable]
    public class InvalidOpCodeException : System.Exception
    {
        public InvalidOpCodeException() { }
        public InvalidOpCodeException(string message) : base(message) { }
        public InvalidOpCodeException(string message, System.Exception inner) : base(message, inner) { }
        public InvalidOpCodeException(byte code)
            : base(string.Format("Invalid operation code: opCode[{0}]", Helper.ToHexString(code)))
        {
        }
        protected InvalidOpCodeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
