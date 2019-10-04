using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Discover.Table;
using Mineral.Utils;

namespace Mineral.Common.Overlay.Discover
{
    public class DiscoverTask : Runnable
    {
        #region Field
        private byte[] node_id;
        private NodeManager node_manager = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public DiscoverTask(NodeManager node_manager)
        {
            this.node_manager = node_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private string DumpNodes()
        {
            string result = "";
            foreach (NodeEntry entry in this.node_manager.Table.GetAllNodes())
            {
                result += "    " + entry.Node.ToString() + "\n";
            }
            return result;
        }
        #endregion


        #region External Method
        public override void Run()
        {
            Discover(this.node_id, 0, new List<Node.Node>());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Discover(byte[] node_id, int round, List<Node.Node> prev_tried)
        {
            try
            {
                if (round == KademliaOptions.MAX_STEPS)
                {
                    Logger.Debug(
                        string.Format("Node table contains [{0}] peers", this.node_manager.Table.GetNodesCount()));

                    Logger.Debug("{}" + 
                        string.Format("(KademliaOptions.MAX_STEPS) Terminating discover after {0} rounds.", round));

                    Logger.Trace("{}\n{}" + 
                        string.Format("Nodes discovered {0} ", this.node_manager.Table.GetNodesCount()) + DumpNodes());

                    return;
                }

                List<Node.Node> closest = this.node_manager.Table.GetClosestNodes(node_id);
                List<Node.Node> tried = new List<Node.Node>();

                foreach (Node.Node n in closest)
                {
                    if (!tried.Contains(n) && !prev_tried.Contains(n))
                    {
                        try
                        {
                            this.node_manager.GetNodeHandler(n).SendFindNode(node_id);
                            tried.Add(n);
                            Thread.Sleep(50);
                        }
                        catch (System.Exception e)
                        {
                            Logger.Error("Unexpected Exception " + e.Message);
                        }
                    }

                    if (tried.Count == KademliaOptions.ALPHA)
                    {
                        break;
                    }
                }

                if (tried.IsNullOrEmpty())
                {
                    Logger.Debug("{}" + 
                        string.Format("(tried.IsNullOrEmpty()) Terminating discover after {0} rounds.", round));

                    Logger.Trace("{}\n{}" + 
                        string.Format("Nodes discovered {0} ", this.node_manager.Table.GetNodesCount()) + DumpNodes());
                    return;
                }

                tried.AddRange(prev_tried);

                Discover(node_id, round + 1, tried);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }
        #endregion
    }
}
