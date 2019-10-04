using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM;
using static Mineral.Common.Runtime.VM.InternalTransaction;

namespace Mineral.Common.Runtime
{
    public interface IRunTime
    {
        TransactionType TransactionType { get; }
        ProgramResult Result { get; }
        string RuntimeError { get;  }

        void Execute();
        void Go();
        void Finalization();
        void SetEnableEventListener(bool enable_event_listener);
        
    }
}
