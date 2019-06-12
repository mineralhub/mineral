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
        #endregion


        #region Constructor
        public BytesCapsule(byte[] bytes)
        {
            this.datas = bytes;
        }

        public object Instance => null;
        public byte[] Data => this.datas;

        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
