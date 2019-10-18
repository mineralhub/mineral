using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Mineral.Core;
using Mineral.Core.Net.Peer;

namespace Mineral.Common.Overlay.Server
{
    public class NettyChannelInitializer : ChannelInitializer<TcpSocketChannel>
    {
        #region Field
        private string remote_id = "";
        private bool is_discovery_mode = false;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public NettyChannelInitializer(string remote_id, bool is_discovery_mode)
        {
            this.remote_id = remote_id;
            this.is_discovery_mode = is_discovery_mode;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        protected override void InitChannel(TcpSocketChannel channel)
        {
            try
            {
                Channel peer = new PeerConnection();
                peer.Init(channel.Pipeline, this.remote_id, this.is_discovery_mode);

                channel.Configuration.RecvByteBufAllocator = new FixedRecvByteBufAllocator(256 * 1024);
                channel.Configuration.SetOption(ChannelOption.SoRcvbuf, 256 * 1024);
                channel.Configuration.SetOption(ChannelOption.SoBacklog, 1024);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }
        #endregion
    }
}
