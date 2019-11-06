using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Discover.Table;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static Mineral.Utils.ScheduledExecutorService;

namespace Mineral.Common.Overlay.Discover
{
    public class DiscoverExecutor
    {
        #region Field
        private ScheduledExecutorHandle discover = null;
        private ScheduledExecutorHandle refresh = null;
        private NodeManager node_manager = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public DiscoverExecutor(NodeManager node_manager)
        {
            this.node_manager = node_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Start()
        {
            this.discover = ScheduledExecutorService.Scheduled(() =>
            {
                new DiscoverTask(this.node_manager);
            }, 1, (int)KademliaOptions.DISCOVER_CYCLE * 1000);

            this.refresh = ScheduledExecutorService.Scheduled(() =>
            {
                new RefreshTask(this.node_manager);
            }, 1, (int)KademliaOptions.BUCKET_REFRESH);
        }

        public void Close()
        {
            this.discover.Shutdown();
            this.refresh.Shutdown();
        }
        #endregion
    }
}
