using System;
using System.Collections.Generic;
using System.Text;
using Protocol;

namespace Mineral.Common.Overlay.Discover.Node.Statistics
{
    public class Reputation
    {
        #region Score<T>
        public abstract class Score<T> : IComparable<Score<T>>
        {
            protected T t;

            public virtual bool IsContinue => true;
            public int Order => 0;

            public Score(T t)
            {
                this.t = t;
            }

            public abstract int Calculate(int base_score);

            public int CompareTo(object obj)
            {
                return GetType().Equals(obj) ? CompareTo(obj as Score<T>) : -1;
            }

            public int CompareTo(Score<T> other)
            {
                if (Order > other.Order)
                {
                    return 1;
                }
                else if (Order < other.Order)
                {
                    return -1;
                }
                return 0;
            }
        }
        #endregion

        #region DiscoverScore : Score<MessageStatistics>
        public class DiscoverScore : Score<MessageStatistics>
        {
            public override bool IsContinue
            {
                get
                {
                    return t.DiscoverOutPing.TotalCount == t.DiscoverInPong.TotalCount 
                        && t.DiscoverInNeighbours.TotalCount <= t.DiscoverOutFindNode.TotalCount;
                }
            }

            public DiscoverScore(MessageStatistics message_statistics)
                : base(message_statistics)
            {
            }

            public override int Calculate(int base_score)
            {
                int discover_reput = base_score;
                discover_reput +=
                    (int)Math.Min(t.DiscoverInPong.TotalCount, 1) * (t.DiscoverOutPing.TotalCount == t.DiscoverInPong.TotalCount ? 101 : 1);
                discover_reput +=
                    (int)Math.Min(t.DiscoverInNeighbours.TotalCount, 1) * (t.DiscoverOutFindNode.TotalCount == t.DiscoverInNeighbours.TotalCount ? 10 : 1);
                return discover_reput;
            }
        }
        #endregion

        #region TcpScore : Score<NodeStatistics>
        public class TcpScore : Score<NodeStatistics>
        {
            public TcpScore(NodeStatistics node_statistics)
                : base(node_statistics)
            {
            }

            public override int Calculate(int base_score)
            {
                int reput = base_score;
                reput += t.P2pHandshake.TotalCount > 0 ? 10 : 0;
                reput += (int)Math.Min(t.TcpFlow.TotalCount / 10240, 20);
                reput += t.MessageStatistics.P2pOutPing.TotalCount == t.MessageStatistics.P2pInPong.TotalCount ? 10 : 0;
                return reput;
            }
        }
        #endregion

        #region DisConnectScore : Score<NodeStatistics>
        public class DisConnectScore : Score<NodeStatistics>
        {
            public DisConnectScore(NodeStatistics node_statistics)
                : base(node_statistics)
            {
            }

            public override int Calculate(int base_score)
            {
                double score = base_score;
                if (t.WasDisconnected)
                {
                    if (t.LastLocalDisconnect == null && t.LastRemoteDisconnect == null)
                    {
                        score *= 0.8;
                    }
                    else if (t.LastLocalDisconnect != ReasonCode.Requested)
                    {
                        if (t.LastRemoteDisconnect == ReasonCode.TooManyPeers
                            || t.LastLocalDisconnect == ReasonCode.TooManyPeers
                            || t.LastRemoteDisconnect == ReasonCode.TooManyPeersWithSameIp
                            || t.LastLocalDisconnect == ReasonCode.TooManyPeersWithSameIp
                            || t.LastRemoteDisconnect == ReasonCode.DuplicatePeer
                            || t.LastLocalDisconnect == ReasonCode.DuplicatePeer
                            || t.LastRemoteDisconnect == ReasonCode.TimeOut
                            || t.LastLocalDisconnect == ReasonCode.TimeOut
                            || t.LastRemoteDisconnect == ReasonCode.PingTimeout
                            || t.LastLocalDisconnect == ReasonCode.PingTimeout
                            || t.LastRemoteDisconnect == ReasonCode.ConnectFail
                            || t.LastLocalDisconnect == ReasonCode.ConnectFail)
                        {
                            score *= 0.9;
                        }
                        else if (t.LastLocalDisconnect == ReasonCode.Reset)
                        {
                            score *= 0.95;
                        }
                        else if (t.LastRemoteDisconnect != ReasonCode.Requested)
                        {
                            score *= 0.7;
                        }
                    }
                }

                if (t.DisconnectTime > 20)
                {
                    return 0;
                }

                return (int)(score - Math.Pow(2, t.DisconnectTime) * (t.DisconnectTime > 0 ? 10 : 0));
            }
        }
        #endregion

        #region OtherScore : Score<NodeStatistics>
        public class OtherScore : Score<NodeStatistics>
        {
            public OtherScore(NodeStatistics node_statistics)
                : base (node_statistics)
            {
            }

            public override int Calculate(int base_score)
            {
                base_score += t.DiscoveryMessageLatency.GetAverage() == 0 ? 
                        0 : (int)Math.Min(1000 / t.DiscoveryMessageLatency.GetAverage(), 20);

                return base_score;
            }
        }
        #endregion


        private List<Score<MessageStatistics>> message_score_list = new List<Score<MessageStatistics>>();
        private List<Score<NodeStatistics>> node_score_list = new List<Score<NodeStatistics>>();

        public Reputation(NodeStatistics nodeStatistics)
        {
            Score<MessageStatistics> discover_score = new DiscoverScore(nodeStatistics.MessageStatistics);
            Score<NodeStatistics> other_score = new OtherScore(nodeStatistics);
            Score<NodeStatistics> tcp_score = new TcpScore(nodeStatistics);
            Score<NodeStatistics> disconnect_score = new DisConnectScore(nodeStatistics);

            message_score_list.Add(discover_score);
            node_score_list.Add(tcp_score);
            node_score_list.Add(other_score);
            node_score_list.Add(disconnect_score);
        }

        public int Calculate()
        {
            bool is_continue = true;
            int score_number = 0;

            foreach (Score<MessageStatistics> score in this.message_score_list)
            {
                score_number = score.Calculate(score_number);
                if (!score.IsContinue)
                {
                    is_continue = false;
                    break;
                }
            }

            if (is_continue)
            {
                foreach (Score<MessageStatistics> score in this.message_score_list)
                {
                    score_number = score.Calculate(score_number);
                    if (!score.IsContinue)
                    {
                        is_continue = false;
                        break;
                    }
                }
            }

            return score_number > 0 ? score_number : 0;
        }
    }
}
