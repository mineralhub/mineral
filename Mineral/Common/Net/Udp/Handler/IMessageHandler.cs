using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Net.Udp.Handler
{
    public interface IMessageHandler
    {
        void Accept(UdpEvent udp_event);
    }
}
