using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Overlay.Message
{
    public abstract class MessageFactory
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public abstract Message Create(byte[] data);
        #endregion
    }
}
