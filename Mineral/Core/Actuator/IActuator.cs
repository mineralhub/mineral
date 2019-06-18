using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;

namespace Mineral.Core.Actuator
{
    public interface IActuator
    {
        bool Execute(TransactionResultCapsule result);
        bool Validate();
        long CalcFee();
        ByteString GetOwnerAddress();
    }
}
