using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Mineral.Common.Overlay
{
    public class TcpSocketChannelEx : TcpSocketChannel
    {
        public TcpSocketChannelEx()
            : base(AddressFamily.InterNetwork)
        {
        }
    }
}
