using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Capsule
{
    public interface IProtoCapsule<T>
    {
        T Instance { get; }
        byte[] Data { get; }
    }
}
