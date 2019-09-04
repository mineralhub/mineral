using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Mineral.Common.Backup;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Service;
using Mineral.Utils;
using Protocol;
using static Mineral.Utils.ScheduledExecutorService;

namespace Mineral.Common.Overlay.Server
{
    using Node = Mineral.Common.Overlay.Discover.Node.Node;

    public class FastForward
    {
        #region Field
        private ScheduledExecutorHandle handle_service = null;

        private List<Node> nodes = Args.Instance.Node.FastForward;
        private byte[] witness_address = Args.Instance.LocalWitness.GetWitnessAccountAddress();
        private int key_size = Args.Instance.LocalWitness.GetPrivateKey().Length;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Connect()
        {
            this.nodes.ForEach(node =>
            {
                IPAddress address = new IPEndPoint(IPAddress.Parse(node.Host), node.Port).Address;
                Manager.Instance.ChannelManager.ActiveNodes.TryAdd(address, node);
            });
        }

        private void Disconnect()
        {
            this.nodes.ForEach(node =>
            {
                IPAddress address = new IPEndPoint(IPAddress.Parse(node.Host), node.Port).Address;
                Manager.Instance.ChannelManager.ActiveNodes.TryRemove(address, out _);

                foreach (Channel channel in Manager.Instance.ChannelManager.ActivePeer)
                {
                    if (channel.Node.Equals(node))
                    {
                        channel.Disconnect(ReasonCode.Reset);
                    }
                }
            });
        }
        #endregion


        #region External Method
        public void Init()
        {
            Logger.Info(
                string.Format("Fast forward config, isWitness: {0}, keySize: {1}, fastForwardNodes: {2}",
                              Args.Instance.IsWitness,
                              this.key_size,
                              this.nodes.Count));

            if (!Args.Instance.IsWitness
                || this.key_size == 0
                || this.nodes.Count == 0)
            {
                return;
            }

            this.nodes = Args.Instance.Node.FastForward.Count > 0 ? Args.Instance.Node.FastForward : this.nodes;
            this.handle_service = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    if (Manager.Instance.DBManager.Witness.Get(this.witness_address) != null
                        && Manager.Instance.BackupManager.Status == BackupManager.BackupStatus.MASTER
                        && !WitnessService.IsNeedSyncCheck)
                    {
                        Connect();
                    }
                    else
                    {
                        Disconnect();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Info("Execute failed." + e.Message);
                }
            }, 0, 60 * 1000 );
        }
        #endregion
    }
}
