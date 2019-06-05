using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Discover;
using Mineral.Core.Capsule;

namespace Mineral.Common.Overlay.Messages
{
    public class HelloMessage : P2pMessage
    {
        #region Field
        private HelloMessage message = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public HelloMessage(byte type, byte[] raw_data)
            : base(type, raw_data)
        {
            this.message = HelloMessage.Parser.ParseFrom(raw_data);
        }

        public HelloMessage(Node node, long timestamp, BlockCapsule block_capsule)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
