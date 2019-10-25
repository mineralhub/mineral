using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Overlay.Discover.Table
{
    public class DistanceComparator : IComparer<NodeEntry>
    {
        private byte[] target_id;

        public DistanceComparator(byte[] targetId)
        {
            this.target_id = targetId;
        }

        public int Compare(NodeEntry x, NodeEntry y)
        {
            int d1 = NodeEntry.GetDistance(this.target_id, x.Node.Id);
            int d2 = NodeEntry.GetDistance(this.target_id, y.Node.Id);

            if (d1 > d2)
            {
                return 1;
            }
            else if (d1 < d2)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
