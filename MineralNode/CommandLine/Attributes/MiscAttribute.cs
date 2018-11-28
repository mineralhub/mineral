using System;
using System.Collections.Generic;
using System.Text;

namespace MineralNode.CommandLine.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class MiscAttribute : Attribute, ICommandLineAttribute
    {
        private string name;
        private string description;

        public string Name { get { return this.name; } set { if (!this.name.Equals(value)) { this.name = value; } } }
        public string Description { get { return this.description; } set { if (!this.description.Equals(value)) { this.description = value; } } }

        public MiscAttribute()
        {
            this.name = string.Empty;
            this.description = string.Empty;
        }
    }
}
