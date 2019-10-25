using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class AccountResourceInsufficientException : System.Exception
    {
        public AccountResourceInsufficientException() { }
        public AccountResourceInsufficientException(string message) : base(message) { }
        public AccountResourceInsufficientException(string message, System.Exception inner) : base(message, inner) { }
        protected AccountResourceInsufficientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
