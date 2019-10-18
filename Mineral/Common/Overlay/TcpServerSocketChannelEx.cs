using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Mineral.Common.Overlay
{
    public class TcpServerSocketChannelEx : TcpServerSocketChannel
    {
        public TcpServerSocketChannelEx()
            : base (AddressFamily.InterNetwork)
        {
        }
    }
}
