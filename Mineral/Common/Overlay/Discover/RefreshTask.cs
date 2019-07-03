using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Discover.Node;

namespace Mineral.Common.Overlay.Discover
{
    public class RefreshTask : DiscoverTask
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public RefreshTask(NodeManager node_manager) : base(node_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method


        public static byte[] GetNodeId()
        {
            byte[] id = new byte[64];
            new Random().NextBytes(id);

            return id;
        }

        public override void Run()
        {
            Discover(GetNodeId(), 0, new List<Node.Node>());
        }
        #endregion
    }
}
