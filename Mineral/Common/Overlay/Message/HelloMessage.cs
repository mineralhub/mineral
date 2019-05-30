using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Discover;

namespace Mineral.Common.Overlay.Message
{
    public class HelloMessage : P2pMessage
    {
        #region Field
        private Protocol.HelloMessage message = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public HelloMessage(byte type, byte[] raw_data)
            : base(type, raw_data)
        {
            this.message = Protocol.HelloMessage.Parser.ParseFrom(raw_data);
        }

        public HelloMessage(Node node, long timestamp, BlockCapsule)
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
