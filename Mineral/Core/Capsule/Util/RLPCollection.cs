using System.Collections.Generic;

namespace Mineral.Core.Capsule.Util
{
    public class RLPCollection : List<IRLPElement>, IRLPElement
    {
        public byte[] RLPData { get; set; }
    }
}