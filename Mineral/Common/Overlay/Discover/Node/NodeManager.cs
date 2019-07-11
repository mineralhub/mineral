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
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Mineral.Common.Net.Udp;
using Mineral.Common.Net.Udp.Handler;
using Mineral.Common.Net.Udp.Message;
using Mineral.Common.Net.Udp.Message.Discover;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Common.Overlay.Discover.Table;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Utils;
using static Mineral.Utils.ScheduledExecutorService;

namespace Mineral.Common.Overlay.Discover.Node
{
    public class NodeManager : IEventHandler
    {
        private class ListenerHandler
        {
            private Dictionary<NodeHandler, object> discovered_nodes = new Dictionary<NodeHandler, object>();
            private IDiscoverListener listener = null;
            private Predicate<NodeStatistics> filter = null;
            private Dictionary<string, NodeHandler> node_handlers = null;

            public ListenerHandler(IDiscoverListener listener, Predicate<NodeStatistics> filter, Dictionary<string, NodeHandler> node_handlers)
            {
                this.listener = listener;
                this.filter = filter;
                this.node_handlers = node_handlers;
            }

            public void CheckAll()
            {
                foreach (NodeHandler handler in this.node_handlers.Values)
                {
                    bool has = this.discovered_nodes.ContainsKey(handler);
                    bool test = this.filter(handler.NodeStatistics);
                    if (!has && test)
                    {
                        this.listener.NodeAppeared(handler);
                        this.discovered_nodes.Add(handler, null);
                    }
                    else if (has && !test)
                    {
                        this.listener.NodeDisappeared(handler);
                        this.discovered_nodes.Remove(handler);
                    }
                }
            }
        }

        #region Field
        private static readonly long LISTENER_REFRESH_RATE = 1000L;
        private static readonly long DB_COMMIT_RATE = 1 * 60 * 1000L;
        private static readonly int MAX_NODES = 2000;
        private static readonly int NODES_TRIM_THRESHOLD = 3000;

        private Action<UdpEvent> message_sender = null;
        private NodeTable table = null;
        private Node node = null;
        private Node home_node = null;
        private Dictionary<string, NodeHandler> node_handlers = new Dictionary<string, NodeHandler>();
        private Dictionary<IDiscoverListener, ListenerHandler> listeners = new Dictionary<IDiscoverListener, ListenerHandler>();
        private List<Node> boot_nodes = new List<Node>();
        private Timer timer_node_manager = null;
        private ScheduledExecutorHandle timer_pong = null;

        private bool is_inited = false;
        private bool is_inbound_known_node = false;
        private bool is_enable_discovery = false;
        #endregion


        #region Property
        public Action<UdpEvent> MessageSender
        {
            get { return this.message_sender; }
        }

        public NodeTable Table
        {
            get { return this.table; }
        }

        public Node PublicHomeNode
        {
            get { return this.home_node; }
        }

        public ScheduledExecutorHandle TimerPong
        {
            get { return this.timer_pong; }
            set { this.timer_pong = value; }
        }
        #endregion


        #region Contructor
        public NodeManager()
        {
            this.is_enable_discovery = Args.Instance.Node.Discovery.Enable ?? false;

            this.home_node = new Node(RefreshTask.GetNodeId(),
                                      Args.Instance.Node.Discovery.ExternalIP,
                                      Args.Instance.Node.ListenPort);

            foreach (string boot in Args.Instance.Seed.IpList)
            {
                this.boot_nodes.Add(Node.InstanceOf(boot));
            }

            Logger.Info(string.Format("home_node : {0}", this.home_node));
            Logger.Info(string.Format("boot_nodes : size= {0}", this.boot_nodes.Count));

            this.table = new NodeTable(this.home_node);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private string GetKey(Node node)
        {
            return GetKey(new IPEndPoint(new IPAddress(Encoding.UTF8.GetBytes(node.Host)), node.Port));
        }

        private string GetKey(IPEndPoint address)
        {
            return address.Address.ToString() + ":" + address.Port.ToString();
        }

        private void TrimTable()
        {
            if (this.node_handlers.Count > NODES_TRIM_THRESHOLD)
            {
                List<NodeHandler> sorted = new List<NodeHandler>(this.node_handlers.Values);
                sorted.OrderBy(handler => handler.Node.Reputation);

                foreach (var item in this.node_handlers.OrderBy(handler => handler.Value.Node.Reputation))
                {
                    this.node_handlers.Remove(item.Key);

                    if (this.node_handlers.Count <= MAX_NODES)
                        break;
                }
            }
        }

        private void DBRead()
        {
            HashSet<Node> nodes = DatabaseManager.Instance.ReadNeighbours();

            Logger.Info(
                "Reading Node statistics from PeersStore : " + nodes.Count + " nodes.");

            foreach (Node node in nodes)
            {
                GetNodeHandler(node).NodeStatistics.PersistedReputation = node.Reputation;
            }
        }

        private void DBWrite()
        {
            HashSet<Node> batch = new HashSet<Node>();

            lock (this)
            {
                foreach (NodeHandler handler in this.node_handlers.Values)
                {
                    int reputation = handler.NodeStatistics.Reputation;
                    handler.Node.Reputation = reputation;
                    batch.Add(handler.Node);
                }
            }

            Logger.Info("Write Node statistics to PeersStore: " + batch.Count + " nodes.");
            DatabaseManager.Instance.ClearAndWriteNeighbours(batch);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ProcessListeners()
        {
            foreach (ListenerHandler handler in listeners.Values)
            {
                try
                {
                    handler.CheckAll();
                }
                catch (Exception e)
                {
                    Logger.Error("Exception processing listener: " + handler);
                }
            }
        }
        #endregion


        #region External Method
        public bool IsNodeAlive(NodeHandler handler)
        {
            return handler.State == NodeHandler.NodeHandlerState.Alive
                || handler.State == NodeHandler.NodeHandlerState.Active
                || handler.State == NodeHandler.NodeHandlerState.EvictCandidate;
        }

        public bool ContainNodeHandler(Node node)
        {
            return this.node_handlers.ContainsKey(GetKey(node));
        }

        public void ChannelActivated()
        {
            if (!this.is_inited)
            {
                this.is_inited = true;
                this.timer_node_manager = new Timer((object obj) =>
                {
                    ProcessListeners();
                }, null, LISTENER_REFRESH_RATE, LISTENER_REFRESH_RATE);

                if (Args.Instance.Node.Discovery.Persist == true)
                {
                    DBRead();
                    this.timer_node_manager = new Timer((object obj) =>
                    {
                        DBWrite();
                    }, null, DB_COMMIT_RATE, DB_COMMIT_RATE);
                }

                foreach (Node node in this.boot_nodes)
                {
                    GetNodeHandler(node);
                }
            }
        }

        public void HandlerEvent(UdpEvent udp_event)
        {
            Message message = udp_event.Message;
            IPEndPoint sender = udp_event.Address;

            Node node = new Node(message.From.Id, sender.Address.ToString(), sender.Port);
            if (this.is_inbound_known_node && !ContainNodeHandler(node))
            {
                Logger.Warning(
                    string.Format("Receive packet from unknown node {0}.", sender.Address.ToString()));

                return;
            }

            NodeHandler handler = GetNodeHandler(node);
            handler.NodeStatistics.MessageStatistics.AddUdpInMessage(message.Type);
            switch (message.Type)
            {
                case UdpMessageType.DISCOVER_PING:
                    {
                        handler.HandlePing((PingMessage)message);
                    }
                    break;
                case UdpMessageType.DISCOVER_PONG:
                    {
                        handler.HandlePong((PongMessage)message);
                    }
                    break;
                case UdpMessageType.DISCOVER_FIND_NODE:
                    {
                        handler.HandleFindNode((FindNodeMessage)message);
                    }
                    break;
                case UdpMessageType.DISCOVER_NEIGHBORS:
                    {
                        handler.HandleNeighbours((NeighborsMessage)message);
                    }
                    break;
                default:
                    break;
            }
        }

        public void SendOutBound(UdpEvent udp_event)
        {
            if (this.is_enable_discovery && this.message_sender != null)
            {
                this.message_sender.Invoke(udp_event);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<NodeHandler> GetNodes(int min_reputation)
        {
            List<NodeHandler> result = new List<NodeHandler>();
            foreach (NodeHandler handler in this.node_handlers.Values)
            {
                if (handler.NodeStatistics.Reputation >= min_reputation)
                {
                    result.Add(handler);
                }
            }

            return result;
        }

        public List<NodeHandler> GetNodes(Func<NodeHandler, HashSet<string>, bool> predicate, HashSet<string> nodes_use, int limit)
        {
            List<NodeHandler> result = new List<NodeHandler>();

            lock (this)
            {
                foreach (NodeHandler handler in this.node_handlers.Values)
                {
                    if (predicate(handler, nodes_use))
                    {
                        result.Add(handler);
                    }
                }
            }

            Logger.Debug(
                string.Format("node_handler size {0} filter peer  size {1}",
                              this.node_handlers.Count,
                              result.Count));

            //TODO: here can use head num sort.
            result = result.OrderBy(handler => handler.NodeStatistics.Reputation).Reverse().ToList();

            return result.Truncate(limit);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public NodeHandler GetNodeHandler(Node node)
        {
            string key = GetKey(node);
            this.node_handlers.TryGetValue(key, out NodeHandler result);

            if (result == null)
            {
                TrimTable();
                result = new NodeHandler(node, this);
                this.node_handlers.Add(key, result);
            }
            else if (result.Node.IsDiscoveryNode && !node.IsDiscoveryNode)
            {
                result.Node = node;
            }

            return result;
        }

        public NodeStatistics GetNodeStatistics(Node node)
        {
            return GetNodeHandler(node).NodeStatistics;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDiscoverListener(IDiscoverListener listener, Predicate<NodeStatistics> filter)
        {
            listeners.Add(listener, new ListenerHandler(listener, filter, this.node_handlers));
        }

        public List<NodeHandler> DumpActiveNodes()
        {
            List<NodeHandler> handlers = new List<NodeHandler>();
            foreach (NodeHandler handler in this.node_handlers.Values)
            {
                if (IsNodeAlive(handler))
                {
                    handlers.Add(handler);
                }
            }

            return handlers;
        }

        public void Close()
        {
            try
            {
                this.timer_node_manager.Change(Timeout.Infinite, Timeout.Infinite);
                this.timer_pong.Cancel();
            }
            catch (System.Exception e)
            {
                Logger.Warning("close fail" + e.Message);
            }
        }
        #endregion
    }
}
