using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;

namespace Mineral.Core.Net.MessageHandler
{
    public interface IMessageHandler
    {
        void ProcessMessage(PeerConnection peer, MineralMessage message);
    }
}
