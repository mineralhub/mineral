using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [System.Serializable]
    public class RevokingStoreIllegalStateException : System.Exception
    {
        public RevokingStoreIllegalStateException() { }
        public RevokingStoreIllegalStateException(string message) : base(message) { }
        public RevokingStoreIllegalStateException(string message, System.Exception inner) : base(message, inner) { }
        protected RevokingStoreIllegalStateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
