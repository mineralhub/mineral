using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Capsule
{
    public interface ICapsule<T>
    {
        byte[] GetData();
        T GetInstance();
    }
}
