using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Net.Udp.Message
{
    public enum UdpMessageType : byte
    {
        DISCOVER_PING = 0x01,
        DISCOVER_PONG = 0x02,
        DISCOVER_FIND_NODE = 0x03,
        DISCOVER_NEIGHBORS = 0x04,
        BACKUP_KEEP_ALIVE = 0x05,
        UNKNOWN = 0xFF
    }
}
