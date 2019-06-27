using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Messages;
using Mineral.Core.Net.Messages;
using static Protocol.Inventory.Types;

namespace Mineral.Common.Overlay.Server
{
    using Message = Mineral.Common.Overlay.Messages.Message;

    public class MessageQueue
    {
        #region Field
        private volatile bool send_message_flag = false;
        private long send_time = 0;
        private Thread thread_send_message = null;
        private Channel channel = null;
        private IChannelHandlerContext context;
        private Queue<MessageRoundTrip> request_queue = new Queue<MessageRoundTrip>();
        private ConcurrentQueue<Message> message_queue = new ConcurrentQueue<Message>();

        private Timer timer_task = null;
        private Stopwatch stopwatch = Stopwatch.StartNew();
        #endregion


        #region Property
        public Channel Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Send()
        {
            MessageRoundTrip round_trip = this.request_queue.Peek();
            if (!this.send_message_flag || round_trip == null)
                return;

            if (round_trip.RetryTime > 0 && !round_trip.HasToRetry())
                return;

            if (round_trip.RetryTime > 0)
            {
                this.channel.Node.NodeDisconnectedLocal(Protocol.ReasonCode.PingTimeout);
                Logger.Warning(
                    string.Format("Wait {0} timeout. close channel {1}.",
                                  round_trip.Message.AnswerMessage,
                                  this.context.Channel.RemoteAddress));
                channel.Close();
                return;
            }

            Message msg = round_trip.Message;

            this.context.WriteAndFlushAsync(msg.GetSendData()).ContinueWith(task =>
            {
                if (!task.IsCompleted)
                {
                    Logger.Error(
                        string.Format("Fail send to {0}, {1}", this.context.Channel.RemoteAddress, msg));
                }
            });

            round_trip.IncreaseRetryTime();
            round_trip.SaveTime();
        }

        private bool NeedToLog(Message msg)
        {
            if (msg is PingMessage
                || msg is PongMessage
                || msg is TransactionsMessage)
            {
                return false;
            }

            if (msg is InventoryMessage
                && ((InventoryMessage)msg).InventoryMessageType.Equals(InventoryType.Trx))
            {
                return false;
            }

            return true;
        }
        #endregion


        #region External Method
        public void Activate(IChannelHandlerContext context)
        {
            this.context = context;
            this.send_message_flag = true;

            this.timer_task = new Timer((object state) =>
            {
                try
                {
                    if (this.send_message_flag)
                    {
                        Send();
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Unhandled exception" + e.Message);
                }
                this.timer_task.Change(10, 0);
            }, this, 10, 0);

            this.thread_send_message = new Thread(new ThreadStart(() =>
            {
                while (this.send_message_flag)
                {
                    try
                    {
                        if (this.message_queue.IsEmpty)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        this.message_queue.TryDequeue(out Message msg);
                        this.context.WriteAndFlushAsync(msg.GetSendData()).ContinueWith(task =>
                        {
                            if (task.IsCompleted && this.channel.IsDisconnect)
                            {
                                Logger.Error(
                                    string.Format("Fail send to {0}, {1}", this.context.Channel.RemoteAddress, msg));
                            }
                        });
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error(
                            string.Format("Fail send to {0}, error info: {1}", this.context.Channel.RemoteAddress, e.Message));
                    }
                }
            }));

            this.thread_send_message.Name = "sendMsgThread-" + this.context.Channel.RemoteAddress;
            this.thread_send_message.Start();
        }

        public bool SendMessage(Message msg)
        {
            if (msg is PingMessage && this.send_time > DateTime.Now.Ticks - 10_000)
                return false;

            if (NeedToLog(msg))
                Logger.Info(string.Format("Send to {0}, {1} ", this.context.Channel.RemoteAddress, msg));

            this.channel.Node.Message.AddTcpOutMessage(msg);
            this.send_time = DateTime.Now.Ticks;

            if (msg.AnswerMessage != null)
            {
                this.request_queue.Enqueue(new MessageRoundTrip(msg));
            }
            else
            {
                this.message_queue.Enqueue(msg);
            }
            return true;
        }

        public void ReceivedMessage(Message msg)
        {
            if (NeedToLog(msg))
            {
                Logger.Info(
                    string.Format("Receive from {0}, {1}", this.context.Channel.RemoteAddress, msg));
            }

            this.channel.Node.Message.AddTcpInMessage(msg);

            MessageRoundTrip round_trip = this.request_queue.Peek();
            if (round_trip != null && round_trip.Message.AnswerMessage == msg.GetType())
            {
                this.request_queue.Dequeue();
            }
        }

        public void Close()
        {
            this.send_message_flag = false;

            if (this.timer_task != null)
            {
                this.timer_task.Change(Timeout.Infinite, Timeout.Infinite);
                this.timer_task.Dispose();
                this.timer_task = null;
            }

            if (this.thread_send_message != null)
            {
                try
                {
                    this.thread_send_message.Join(20);
                    this.thread_send_message = null;
                }
                catch (Exception e)
                {
                    Logger.Warning(
                        string.Format("Join send thread failed, peer {0}", this.context.Channel.RemoteAddress));
                }
            }
        }
        #endregion
    }
}
