using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.CommandLine.Attributes
{
    public interface ICommandLineAttribute
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}
