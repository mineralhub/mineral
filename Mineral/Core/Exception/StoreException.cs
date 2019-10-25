using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class StoreException : System.Exception
    {
        public StoreException() { }
        public StoreException(string message) : base(message) { }
        public StoreException(string message, System.Exception inner) : base(message, inner) { }
        protected StoreException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
