using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Capsule
{
    public class BytesCapsule : IProtoCapsule<object>
    {
        #region Field
        byte[] datas = null;
        #endregion


        #region Property
        object Instance { get { return null; } }
        byte[] Data { get { return this.datas; } }
        #endregion


        #region Constructor
        public BytesCapsule(byte[] bytes)
        {
            this.datas = bytes;
        }

        public object Instance { get { return null; } }
        public byte[] Data { get { return this.Data; } }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
