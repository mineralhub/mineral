using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Program.Listener
{
    public interface IProgramListenerAware
    {
        void SetProgramListener(IProgramListener listener);
    }
}
