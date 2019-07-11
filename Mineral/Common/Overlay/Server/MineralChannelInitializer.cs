using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Channels;

namespace Mineral.Common.Overlay.Server
{
    public class MineralChannelInitializer : ChannelInitializer<IChannel>
    {
        #region Field
        private ChannelManager channel_manager = null;
        private string remote_id = "";
        private bool peer_discovery_mode = false;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public MineralChannelInitializer(ChannelManager channel_manager, string remote_id)
        {
            this.channel_manager = channel_manager;
            this.remote_id = remote_id;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        protected override void InitChannel(IChannel channel)
        {
            try
            {
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }
        #endregion
    }
}
