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
using System.Net;
using System.Text;
using System.Threading;
using Mineral.Common.Net.Udp.Handler;
using Mineral.Common.Net.Udp.Message;
using Mineral.Common.Net.Udp.Message.Discover;
using Mineral.Common.Overlay.Discover.Node.Statistics;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;

namespace Mineral.Common.Overlay.Discover.Node
{
    public class NodeHandler
    {
        public enum NodeHandlerState
        {
            Discovered,
            Dead,
            Alive,
            Active,
            EvictCandidate,
            NonActive
        }

        #region Field
        private static int PingTimeout = 15000;
        private Node source_node = null;
        private Node node = null;
        private NodeHandlerState state = NodeHandlerState.Discovered;
        private NodeManager node_manager = null;
        private NodeStatistics node_statistics = null;
        private NodeHandler replace_candidate = null;
        private IPEndPoint socket_address = null;
        private int ping_trials = 3;
        private volatile bool wait_pong = false;
        private volatile bool wait_neighbors = false;
        private long ping_sent = 0;
        private long ping_sequence = 0;
        private long findnode_sequence = 0;
        #endregion


        #region Property
        public IPEndPoint SocketAddress
        {
            get { return this.socket_address; }
        }

        public Node SourceNode
        {
            get { return this.source_node; }
            set { this.source_node = value; }
        }

        public Node Node
        {
            get { return this.node; }
            set { this.node = value; }
        }

        public NodeHandlerState State
        {
            get { return this.state; }
        }

        public NodeStatistics NodeStatistics
        {
            get { return this.node_statistics; }
        }
        #endregion


        #region Contructor
        public NodeHandler(Node node, NodeManager node_manager)
        {
            this.node = node;
            this.node_manager = node_manager;
            this.socket_address = new IPEndPoint(new IPAddress(Encoding.UTF8.GetBytes(node.Host)), node.Port);
            this.node_statistics = new NodeStatistics(node);
            ChangeState(NodeHandlerState.Discovered);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void ChallengeWith(NodeHandler replace_candidate)
        {
            this.replace_candidate = replace_candidate;
            ChangeState(NodeHandlerState.EvictCandidate);
        }

        private void SendMessage(Message message)
        {
            this.node_manager.SendOutBound(new UdpEvent(message, this.socket_address));
            this.node_statistics.MessageStatistics.AddUdpOutMessage(message.Type);
        }
        #endregion


        #region External Method
        public void ChangeState(NodeHandlerState new_state)
        {
            NodeHandlerState old_state = this.state;
            if (new_state == NodeHandlerState.Discovered)
            {
                if (this.source_node != null && this.source_node.Port != this.node.Port)
                {
                    ChangeState(NodeHandlerState.Dead);
                }
                else
                {
                    SendPing();
                }
            }
            if (!node.IsDiscoveryNode)
            {
                if (new_state == NodeHandlerState.Alive)
                {
                    Node evict_candidate = this.node_manager.Table.AddNode(this.node);
                    if (evict_candidate == null)
                    {
                        new_state = NodeHandlerState.Active;
                    }
                    else
                    {
                        NodeHandler evict_handler = this.node_manager.GetNodeHandler(evict_candidate);
                        if (evict_handler.State != NodeHandlerState.EvictCandidate)
                        {
                            evict_handler.ChallengeWith(this);
                        }
                    }
                }
                if (new_state == NodeHandlerState.Active)
                {
                    if (old_state == NodeHandlerState.Alive)
                    {
                        this.node_manager.Table.AddNode(node);
                    }
                    else if (old_state == NodeHandlerState.EvictCandidate)
                    {
                    }
                    else
                    {
                    }
                }

                if (new_state == NodeHandlerState.NonActive)
                {
                    if (old_state == NodeHandlerState.EvictCandidate)
                    {
                        this.node_manager.Table.DropNode(node);
                        this.replace_candidate.ChangeState(NodeHandlerState.Active);
                    }
                    else if (old_state == NodeHandlerState.Alive)
                    {
                    }
                    else
                    {
                    }
                }
            }

            if (new_state == NodeHandlerState.EvictCandidate)
            {
                SendPing();
            }
            state = new_state;
        }

        public void HandlePing(PingMessage message)
        {
            if (!this.node_manager.Table.Node.Equals(node))
            {
                SendPong(message.Timestamp);
            }
            if (message.Version != Args.Instance.Node.P2P.Version)
            {
                ChangeState(NodeHandlerState.NonActive);
            }
            else if (this.state.Equals(NodeHandlerState.NonActive) || this.state.Equals(NodeHandlerState.Dead))
            {
                ChangeState(NodeHandlerState.Discovered);
            }
        }

        public void HandlePong(PongMessage message)
        {
            if (this.wait_pong)
            {
                this.wait_pong = false;

                this.node_statistics.DiscoveryMessageLatency.Add((double)(Helper.CurrentTimeMillis() - ping_sent));
                this.node_statistics.LastPongReplyTime = Helper.CurrentTimeMillis();

                this.node.Id = message.From.Id;
                if (message.Version != Args.Instance.Node.P2P.Version)
                {
                    ChangeState(NodeHandlerState.NonActive);
                }
                else
                {
                    ChangeState(NodeHandlerState.Alive);
                }
            }
        }

        public void HandleNeighbours(NeighborsMessage message)
        {
            if (!this.wait_neighbors)
            {
                Logger.Warning(
                    string.Format("Receive neighbors from {0} without send find nodes.", node.Host));

                return;
            }

            this.wait_neighbors = false;

            foreach (Node node in message.Nodes)
            {
                if (!this.node_manager.PublicHomeNode.Id.ToHexString().Equals(node.Id.ToHexString()))
                {
                    this.node_manager.GetNodeHandler(node);
                }
            }
        }

        public void HandleFindNode(FindNodeMessage message)
        {
            List<Node> closest = this.node_manager.Table.GetClosestNodes(message.TargetId);
            SendNeighbours(closest, message.Timestamp);
        }

        public void HandleTimedOut()
        {
            this.wait_pong = false;
            if (Interlocked.Decrement(ref this.ping_trials) > 0)
            {
                SendPing();
            }
            else
            {
                if (this.state == NodeHandlerState.Discovered)
                {
                    ChangeState(NodeHandlerState.Dead);
                }
                else if (this.state == NodeHandlerState.EvictCandidate)
                {
                    ChangeState(NodeHandlerState.NonActive);
                }
                else
                {
                }
            }
        }

        public void SendPing()
        {
            PingMessage message = new PingMessage(this.node_manager.PublicHomeNode, this.node);

            this.ping_sequence = message.Timestamp;
            this.wait_pong = true;
            this.ping_sent = Helper.CurrentTimeMillis();

            SendMessage(message);

            if (this.node_manager.TimerPong.IsShutdown)
            {
                return;
            }

            this.node_manager.TimerPong = ScheduledExecutorService.Scheduled(() =>
            {
                try
                {
                    if (this.wait_pong)
                    {
                        this.wait_pong = false;
                        HandleTimedOut();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Error("Unhandled exception " + e.Message);
                }
            }, 0, (int)PingTimeout);
        }

        public void SendPong(long sequence)
        {
            Message pong = new PongMessage(this.node_manager.PublicHomeNode, sequence);
            SendMessage(pong);
        }

        public void SendFindNode(byte[] target)
        {
            this.wait_neighbors = true;
            FindNodeMessage message = new FindNodeMessage(this.node_manager.PublicHomeNode, target);

            this.findnode_sequence = message.Timestamp;
            SendMessage(message);
        }

        public void SendNeighbours(List<Node> neighbours, long sequence)
        {
            Message msg = new NeighborsMessage(this.node_manager.PublicHomeNode, neighbours, sequence);
            SendMessage(msg);
        }

        public override string ToString()
        {
            return "NodeHandler[state: " + this.state + ", node: " + this.node.Host + ":" + this.node.Port + "]";
        }
    }
    #endregion

}