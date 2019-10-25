using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ValidateScheduleException : System.Exception
    {
        public ValidateScheduleException() { }
        public ValidateScheduleException(string message) : base(message) { }
        public ValidateScheduleException(string message, System.Exception inner) : base(message, inner) { }
        protected ValidateScheduleException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
