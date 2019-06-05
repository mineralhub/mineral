using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Messages;

namespace Mineral.Core.Net.Messages
{
    public abstract class MineralMessage : Message
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public MineralMessage() { }
        public MineralMessage(byte[] raw_data) : base(raw_data) { }
        public MineralMessage(byte type, byte[] raw_data) : base(type, raw_data) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
