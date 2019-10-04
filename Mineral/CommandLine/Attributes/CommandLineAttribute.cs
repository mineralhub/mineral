using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.CommandLine.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class CommandLineAttribute : Attribute, ICommandLineAttribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
