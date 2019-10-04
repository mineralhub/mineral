using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;


namespace Mineral.Common.Overlay.Discover.Table
{
    using Node = Mineral.Common.Overlay.Discover.Node.Node;

    public class NodeTable
    {
        #region Field
        private Node node = null;

        [NonSerialized]
        private NodeBucket[] buckets = null;
        [NonSerialized]
        private List<NodeEntry> nodes = null;
        #endregion


        #region Property
        public Node Node
        {
            get { return this.node; }
        }

        public NodeBucket[] GetBuckets
        {
            get { return this.buckets; }
        }
        #endregion


        #region Contructor
        public NodeTable(Node node)
        {
            this.node = node;
            Initialize();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Initialize()
        {
            this.nodes = new List<NodeEntry>();
            this.buckets = new NodeBucket[KademliaOptions.BINS];

            for (int i = 0; i < KademliaOptions.BINS; i++)
                buckets[i] = new NodeBucket(i);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Node AddNode(Node n)
        {
            NodeEntry entry = new NodeEntry(this.node.Id, n);
            if (this.nodes.Contains(entry))
            {
                this.nodes.ForEach(item =>
                {
                    if (item.Equals(entry))
                        item.Touch();
                });
                return null;
            }

            NodeEntry last_seen = this.buckets[GetBucketId(entry)].AddNode(entry);
            if (last_seen != null)
            {
                return last_seen.Node;
            }
            if (!this.nodes.Contains(entry))
            {
                this.nodes.Add(entry);
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DropNode(Node n)
        {
            NodeEntry entry = new NodeEntry(node.Id, n);
            this.buckets[GetBucketId(entry)].DropNode(entry);
            this.nodes.Remove(entry);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(Node n)
        {
            NodeEntry entry = new NodeEntry(node.Id, n);
            foreach (NodeBucket b in this.buckets)
            {
                if (b.Nodes.Contains(entry))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void TouchNode(Node n)
        {
            NodeEntry entry = new NodeEntry(node.Id, n);
            foreach (NodeBucket b in this.buckets)
            {
                if (b.Nodes.Contains(entry))
                {
                    b.Nodes[b.Nodes.IndexOf(entry)].Touch();
                    break;
                }
            }
        }

        public int GetBucketsCount()
        {
            int i = 0;
            foreach (NodeBucket b in this.buckets)
            {
                if (b.GetNodesCount() > 0)
                {
                    i++;
                }
            }

            return i;
        }

        public int GetBucketId(NodeEntry entry)
        {
            int id = entry.Distance - 1;

            return id < 0 ? 0 : id;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetNodesCount()
        {
            return nodes.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<NodeEntry> GetAllNodes()
        {
            List<NodeEntry> nodes = new List<NodeEntry>();

            foreach (NodeBucket bucket in this.buckets)
            {
                foreach (NodeEntry entry in bucket.Nodes)
                {
                    if (!entry.Node.Equals(node))
                    {
                        nodes.Add(entry);
                    }
                }
            }

            return nodes;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<Node> GetClosestNodes(byte[] target_id)
        {
            List<NodeEntry> closest_entries = GetAllNodes();
            List<Node> closest_nodes = new List<Node>();

            closest_entries.Sort(new DistanceComparator(target_id));

            if (closest_entries.Count > KademliaOptions.BUCKET_SIZE)
            {
                closest_entries = closest_entries.GetRange(0, KademliaOptions.BUCKET_SIZE);
            }

            foreach (NodeEntry entry in closest_entries)
            {
                if (!entry.Node.IsDiscoveryNode)
                {
                    closest_nodes.Add(entry.Node);
                }
            }

            return closest_nodes;
        }
        #endregion
    }
}
