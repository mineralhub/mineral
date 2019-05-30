using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Overlay.Message
{
    public abstract class P2pMessage : Message
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public P2pMessage() { }
        public P2pMessage(byte[] raw_data) : base(raw_data) { }
        public P2pMessage(byte type, byte[] raw_data) : base(type, raw_data) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
