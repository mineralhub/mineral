/*
 * Copyright (c) [2016] [ <ether.camp> ]
 * This file is part of the ethereumJ library.
 *
 * The ethereumJ library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The ethereumJ library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the ethereumJ library. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mineral.Common.Overlay.Discover.Table
{
    public class NodeBucket
    {
        #region Field
        private int depth = 0;
        private List<NodeEntry> nodes = new List<NodeEntry>();
        #endregion


        #region Property
        public List<NodeEntry> Nodes
        {
            get { return this.nodes; }
        }
        #endregion


        #region Contructor
        public NodeBucket(int depth)
        {
            this.depth = depth;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private NodeEntry GetLastSeen()
        {
            List<NodeEntry> sorted = new List<NodeEntry>(nodes);
            sorted.Sort(new TimeComparator());

            return sorted.Count > 0 ? sorted[0] : null;
        }

        #endregion


        #region External Method
        public int getDepth()
        {
            return depth;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public NodeEntry AddNode(NodeEntry entry)
        {
            if (!this.nodes.Contains(entry))
            {
                if (this.nodes.Count >= KademliaOptions.BUCKET_SIZE)
                {
                    return GetLastSeen();
                }
                else
                {
                    this.nodes.Add(entry);
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DropNode(NodeEntry entry)
        {
            foreach (NodeEntry node in this.nodes)
            {
                if (node.Id.Equals(entry.Id))
                {
                    this.nodes.Remove(node);
                    break;
                }
            }
        }

        public int GetNodesCount()
        {
            return this.nodes.Count;
        }
        #endregion
    }
}
