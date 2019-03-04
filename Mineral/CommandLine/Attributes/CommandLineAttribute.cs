using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.CommandLine.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class CommandLineAttribute : Attribute, ICommandLineAttribute
    {
        private string _name;
        private string _description;

        public string Name { get { return _name; } set { if (!_name.Equals(value)) { _name = value; } } }
        public string Description { get { return _description; } set { if (!_description.Equals(value)) { _description = value; } } }

        public CommandLineAttribute()
        {
            _name = string.Empty;
            _description = string.Empty;
        }
    }
}
