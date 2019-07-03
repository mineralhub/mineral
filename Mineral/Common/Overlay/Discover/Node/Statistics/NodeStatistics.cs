using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mineral.Core.Config.Arguments;
using Protocol;

namespace Mineral.Common.Overlay.Discover.Node.Statistics
{
    public class NodeStatistics
    {
        #region Field
        public static readonly int REPUTATION_PREDEFINED = 100000;
        public static readonly long TOO_MANY_PEERS_PENALIZE_TIMEOUT = 60 * 1000L;
        private static readonly long CLEAR_CYCLE_TIME = 60 * 60 * 1000L;
        private readonly long MIN_DATA_LENGTH = Args.Instance.Node.ReceiveTcpMinDataLength;

        private bool is_predefined = false;
        private int persisted_reputation = 0;
        private int disconnect_times = 0;
        private ReasonCode? last_remote_disconnect = ReasonCode.Unknown;
        private ReasonCode? last_local_disconnect = ReasonCode.Unknown;
        private long last_disconnect_time = 0;
        private long first_disconnect_time = 0;

        private readonly MessageStatistics message_statistics = new MessageStatistics();
        private readonly MessageCount p2p_handshake = new MessageCount();
        private readonly MessageCount tcp_flow = new MessageCount();

        private readonly SimpleStatter discovery_message_latency;
        private long last_pong_reply_time = 0;

        private Reputation reputation = null;
        #endregion


        #region Property
        public MessageStatistics MessageStatistics
        {
            get { return this.message_statistics; }
        }

        public MessageCount P2pHandshake
        {
            get { return this.p2p_handshake; }
        }

        public MessageCount TcpFlow
        {
            get { return this.tcp_flow; }
        }

        public bool IsPreDefine
        {
            get { return this.is_predefined; }
            set { this.is_predefined = value; }
        }

        public int PersistedReputation
        {
            get { return this.persisted_reputation; }
            set { this.persisted_reputation = value; }
        }

        public bool WasDisconnected
        {
            get { return this.last_disconnect_time > 0; }
        }

        public int DisconnectTime
        {
            get { return this.disconnect_times; }
        }

        public ReasonCode? LastRemoteDisconnect
        {
            get { return this.last_remote_disconnect; }
        }

        public ReasonCode? LastLocalDisconnect
        {
            get { return this.last_local_disconnect; }
        }

        public long LastPongReplyTime
        {
            get { return this.last_pong_reply_time; }
            set { Interlocked.Exchange(ref this.last_pong_reply_time, value); }
        }

        public int Reputation
        {
            get
            {
                int score = 0;
                if (!IsReputationPenalized())
                {
                    score += this.persisted_reputation / 5 + this.reputation.Calculate();
                }

                if (this.is_predefined)
                {
                    score += REPUTATION_PREDEFINED;
                }

                return score;
            }
        }

        public SimpleStatter DiscoveryMessageLatency
        {
            get { return this.discovery_message_latency; }
        }
        #endregion


        #region Constructor
        public NodeStatistics(Node node)
        {
            this.discovery_message_latency = new SimpleStatter(node.Id.ToString());
            this.reputation = new Reputation(this);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public ReasonCode? GetDisconnectReason()
        {
            if (this.last_local_disconnect != null)
                return this.last_local_disconnect;
            if (this.last_remote_disconnect != null)
                return this.last_remote_disconnect;

            return ReasonCode.Unknown;
        }

        public bool IsReputationPenalized()
        {
            if (WasDisconnected && last_remote_disconnect == ReasonCode.TooManyPeers
                && Helper.CurrentTimeMillis() - this.last_disconnect_time < TOO_MANY_PEERS_PENALIZE_TIMEOUT)
            {
                return true;
            }

            if (WasDisconnected && last_remote_disconnect == ReasonCode.DuplicatePeer
                && Helper.CurrentTimeMillis() - this.last_disconnect_time < TOO_MANY_PEERS_PENALIZE_TIMEOUT)
            {
                return true;
            }

            if (this.first_disconnect_time > 0
                && (Helper.CurrentTimeMillis() - this.first_disconnect_time) > CLEAR_CYCLE_TIME)
            {
                this.last_local_disconnect = null;
                this.last_remote_disconnect = null;
                this.disconnect_times = 0;
                this.persisted_reputation = 0;
                this.first_disconnect_time = 0;
            }

            if (this.last_local_disconnect == ReasonCode.IncompatibleProtocol
                || this.last_remote_disconnect == ReasonCode.IncompatibleProtocol
                || this.last_local_disconnect == ReasonCode.BadProtocol
                || this.last_remote_disconnect == ReasonCode.BadProtocol
                || this.last_local_disconnect == ReasonCode.BadBlock
                || this.last_remote_disconnect == ReasonCode.BadBlock
                || this.last_local_disconnect == ReasonCode.BadTx
                || this.last_remote_disconnect == ReasonCode.BadTx
                || this.last_local_disconnect == ReasonCode.Forked
                || this.last_remote_disconnect == ReasonCode.Forked
                || this.last_local_disconnect == ReasonCode.Unlinkable
                || this.last_remote_disconnect == ReasonCode.Unlinkable
                || this.last_local_disconnect == ReasonCode.IncompatibleChain
                || this.last_remote_disconnect == ReasonCode.IncompatibleChain
                || this.last_local_disconnect == ReasonCode.SyncFail
                || this.last_remote_disconnect == ReasonCode.SyncFail
                || this.last_local_disconnect == ReasonCode.IncompatibleVersion
                || this.last_remote_disconnect == ReasonCode.IncompatibleVersion)
            {
                this.persisted_reputation = 0;
                return true;
            }
            return false;
        }

        public void NodeDisconnectedRemote(ReasonCode reason)
        {
            this.last_disconnect_time = Helper.CurrentTimeMillis();
            this.last_remote_disconnect = reason;
        }

        public void NodeDisconnectedLocal(ReasonCode reason)
        {
            this.last_disconnect_time = Helper.CurrentTimeMillis();
            this.last_local_disconnect = reason;
        }

        public void NodifyDisconnect()
        {
            this.last_disconnect_time = Helper.CurrentTimeMillis();
            if (this.first_disconnect_time <= 0)
                this.first_disconnect_time = last_disconnect_time;

            if (this.last_local_disconnect == ReasonCode.Reset)
                return;

            this.disconnect_times++;
            this.persisted_reputation = this.persisted_reputation / 2;
        }

        public override string ToString()
        {
            return "NodeStat[reput: " + Reputation + "(" + this.persisted_reputation + "), discover: "
                + this.message_statistics.DiscoverInPong + "/" + this.message_statistics.DiscoverOutPing + " "
                + this.message_statistics.DiscoverOutPong + "/" + this.message_statistics.DiscoverInPing + " "
                + this.message_statistics.DiscoverInNeighbours + "/" + this.message_statistics.DiscoverOutFindNode
                + " "
                + this.message_statistics.DiscoverOutNeighbours + "/" + this.message_statistics.DiscoverInFindNode
                + " "
                + ((int)discovery_message_latency.GetAverage()) + "ms"
                + ", p2p: " + this.p2p_handshake + "/" + this.message_statistics.P2pInHello + "/"
                + this.message_statistics.P2pOutHello + " "
                + ", tron: " + this.message_statistics.MineralInMessage + "/" + this.message_statistics.MineralOutMessage
                + " "
                + (WasDisconnected ? "X " + this.disconnect_times : "")
                + (this.last_local_disconnect != null ? ("<=" + this.last_local_disconnect) : " ")
                + (this.last_remote_disconnect != null ? ("=>" + this.last_remote_disconnect) : " ")
                + ", tcp flow: " + this.tcp_flow.TotalCount;
        }

        #endregion
    }
}
