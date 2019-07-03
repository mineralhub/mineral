using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Net.Udp.Handler
{
    public class UdpEvent
    {
        #region Field
        #endregion


        #region Property
        public Message.Message Message { get; set; }
        public System.Net.IPEndPoint Address { get; set; }
        #endregion


        #region Contructor
        public UdpEvent(Message.Message message, System.Net.IPEndPoint address)
        {
            this.Message = message;
            this.Address = address;
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
