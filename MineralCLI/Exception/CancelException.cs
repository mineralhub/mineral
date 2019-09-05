using System;
using System.Collections.Generic;
using System.Text;

namespace MineralCLI.Exception
{
    [Serializable]
    public class CancelException : System.Exception
    {
        public CancelException() { }
        public CancelException(string message) : base(message) { }
        public CancelException(string message, System.Exception inner) : base(message, inner) { }
        protected CancelException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
