using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class KeyStoreException : System.Exception
    {
        public KeyStoreException() { }
        public KeyStoreException(string message) : base(message) { }
        public KeyStoreException(string message, System.Exception inner) : base(message, inner) { }
        protected KeyStoreException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
