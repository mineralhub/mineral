using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Net.Udp.Handler;

namespace Mineral.Common.Net.Udp
{
    public interface IEventHandler
    {
        void ChannelActivated();
        void HandlerEvent(UdpEvent udp_event);
    }
}
