using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Trace
{
    public partial class OpActions
    {
        public class Action
        {
            public enum Name
            {
                Pop,
                Push,
                Swap,
                Extend,
                Write,
                Put,
                Remove,
                Clear,
                Empty,
            }

            private Name name = Name.Empty;
            private Dictionary<string, object> parameters = null;

            public Name ActionName
            {
                get { return this.name; }
                set { this.name = value; }
            }

            public Dictionary<string, object> Parameters
            {
                get { return this.parameters; }
                set { this.parameters = value; }
            }

            public Action AddParameter(string name, object value)
            {
                if (value != null)
                {
                    if (this.parameters == null)
                    {
                        this.parameters = new Dictionary<string, object>();
                    }
                    this.parameters.Put(name, value.ToString());
                }

                return this;
            }
        }
    }
}
