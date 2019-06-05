using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class PermissionException : System.Exception
    {
        public PermissionException() { }
        public PermissionException(string message) : base(message) { }
        public PermissionException(string message, System.Exception inner) : base(message, inner) { }
        protected PermissionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
