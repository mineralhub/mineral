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
using System.Text;

/**
 * Created by kest on 5/25/15.
 */

namespace Mineral.Common.Overlay.Discover.Table
{
    using Node = Mineral.Common.Overlay.Discover.Node.Node;

    public class NodeEntry
    {
        #region Field
        private byte[] owner_id = null;
        private Node node = null;
        private string entry_id = null;
        private int distance = 0;
        private long modified = 0;
        #endregion


        #region Property
        public string Id
        {
            get { return this.entry_id; }
        }

        public Node Node
        {
            get { return this.node; }
        }

        public int Distance
        {
            get { return this.distance; }
        }

        public long Modified
        {
            get { return this.modified; }
        }
        #endregion


        #region Contructor
        public NodeEntry(Node node)
        {
            this.node = node;
            this.owner_id = node.Id;
            this.entry_id = node.Host;
            this.distance = GetDistance(this.owner_id, node.Id);
            Touch();
        }

        public NodeEntry(byte[] owner_id, Node node)
        {
            this.node = node;
            this.owner_id = owner_id;
            this.entry_id = node.Host;
            this.distance = GetDistance(owner_id, node.Id);
            Touch();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Touch()
        {
            this.modified = Helper.CurrentTimeMillis();
        }

        public static int GetDistance(byte[] owner_id, byte[] target_id)
        {
            byte[] h1 = target_id;
            byte[] h2 = owner_id;
            byte[] hash = new byte[Math.Min(h1.Length, h2.Length)];

            for (int i = 0; i < hash.Length; i++)
            {
                hash[i] = (byte)(((int)h1[i]) ^ ((int)h2[i]));
            }

            int d = KademliaOptions.BINS;

            foreach (byte b in hash)
            {
                if (b == 0)
                {
                    d -= 8;
                }
                else
                {
                    int count = 0;
                    for (int i = 7; i >= 0; i--)
                    {
                        bool a = ((b & 0xff) & (1 << i)) == 0;
                        if (a)
                        {
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    d -= count;

                    break;
                }
            }
            return d;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool ret = false;

            if (obj != null && this.GetType() == obj.GetType())
            {
                NodeEntry e = (NodeEntry)obj;
                ret = this.entry_id.Equals(e.entry_id);
            }

            return ret;
        }
        #endregion
    }
}
