using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Core.Config;
using Mineral.Utils;
using static Mineral.Utils.ScheduledExecutorService;

namespace Mineral.Core.Net.Peer
{
    public class PeerStatusCheck
    {
        #region Field
        private ScheduledExecutorHandle handler_peer_status = null;
        private int block_update_timeout = 20000;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            this.handler_peer_status = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    StatusCheck();
                }
                catch (System.Exception e)
                {
                    Logger.Error("Unhandled exception. " + e.Message);
                }
            }, 5000, 2000);
        }

        public void StatusCheck()
        {
            long now = Helper.CurrentTimeMillis();

            Manager.Instance.NetDelegate.ActivePeers.ForEach(peer =>
            {
                bool is_disconnected = false;

                if (peer.IsNeedSyncPeer
                    && peer.BlockBothHaveTimestamp < now - this.block_update_timeout)
                {
                    Logger.Warning(
                        string.Format("Peer {0} not sync for a long time.", peer.Address.ToString()));

                    is_disconnected = true;
                }

                if (!is_disconnected)
                {
                    List<long> search = new List<long>(peer.InventoryRequest.Values);
                    is_disconnected = search.Where(time => time < now - Parameter.NetParameters.ADV_TIME_OUT).Count() > 0;
                }

                if (!is_disconnected)
                {
                    List<long> search = new List<long>(peer.InventoryRequest.Values);
                    is_disconnected = search.Where(time => time < now - Parameter.NetParameters.SYNC_TIME_OUT).Count() > 0;
                }

                if (is_disconnected)
                {
                    peer.Disconnect(Protocol.ReasonCode.TimeOut);
                }
            });
        }

        public void Close()
        {
            this.handler_peer_status.Shutdown();
        }
        #endregion
    }
}
