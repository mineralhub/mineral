using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DotNetty.Transport.Channels;
using Mineral.Common.Overlay.Messages;
using Mineral.Core.Net.Messages;
using Mineral.Utils;
using static Mineral.Utils.ScheduledExecutorService;
using static Protocol.Inventory.Types;

namespace Mineral.Common.Overlay.Server
{
    using Message = Mineral.Common.Overlay.Messages.Message;

    public class MessageQueue
    {
        #region Field
        private volatile bool send_message_flag = false;
        private long time_send_ping = 0;
        private Thread thread_send_message = null;
        private Channel channel = null;
        private IChannelHandlerContext context;
        private ConcurrentQueue<MessageRoundTrip> request_queue = new ConcurrentQueue<MessageRoundTrip>();
        private ConcurrentQueue<Message> message_queue = new ConcurrentQueue<Message>();
        private ScheduledExecutorService executor = new ScheduledExecutorService();
        private ScheduledExecutorHandle executor_handler = null;
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
            if (this.request_queue.TryPeek(out MessageRoundTrip round_trip))
            {
                if (round_trip == null)
                    return;

                if (!this.send_message_flag)
                    return;

                if (round_trip.RetryTime > 0 && !round_trip.HasToRetry())
                    return;

                if (round_trip.RetryTime > 0)
                {
                    this.channel.NodeStatistics.NodeDisconnectedLocal(Protocol.ReasonCode.PingTimeout);
                    Logger.Warning(
                        string.Format("Wait {0} timeout. close channel {1}.",
                                      round_trip.Message.AnswerMessage,
                                      this.context.Channel.RemoteAddress));
                    channel.Close();

                    return;
                }

                if (round_trip.Message is PingMessage)
                {
                    this.time_send_ping = Helper.CurrentTimeMillis();
                }

                this.context.WriteAndFlushAsync(round_trip.Message.GetSendData()).ContinueWith(task =>
                {
                    if (!task.IsCompleted)
                    {
                        Logger.Error(
                            string.Format("Fail send to {0}, {1}", this.context.Channel.RemoteAddress, round_trip.Message));
                    }
                    else
                    {
                        Logger.Debug(
                            string.Format("Send reqeust message {0}", round_trip.Message.Type.ToString()));
                    }
                });

                round_trip.IncreaseRetryTime();
                round_trip.SaveTime();
            }
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

            this.executor_handler = ScheduledExecutorService.Scheduled(() =>
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
                    Logger.Error("Unhandled exception " + e.Message);
                }
            }, 10, 10);

            this.thread_send_message = new Thread(new ThreadStart(() =>
            {
                while (this.send_message_flag)
                {
                    try
                    {
                        if (this.message_queue.TryDequeue(out Message message))
                        {
                            this.context.WriteAndFlushAsync(message.GetSendData()).ContinueWith(task =>
                            {
                                if (!task.IsCompleted && !this.channel.IsDisconnect)
                                {
                                    Logger.Error(
                                        string.Format("Fail send to {0}, {1}", this.context.Channel.RemoteAddress, message));
                                }
                                else
                                {
                                    Logger.Debug(
                                        string.Format("Send reqeust message {0}", message.Type.ToString()));
                                }
                            });
                        }
                        else
                        {
                            Thread.Sleep(10);
                            continue;
                        }
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

        public bool SendMessage(Message message)
        {
            if (message is PingMessage 
                && this.time_send_ping > Helper.CurrentTimeMillis() - 10000)
            {
                    return false;
            }

            if (NeedToLog(message))
            {
                Logger.Info(string.Format("Send to {0}, {1} ", this.context.Channel.RemoteAddress, message));
            }

            this.channel.NodeStatistics.MessageStatistics.AddTcpOutMessage(message);

            if (message.AnswerMessage != null)
            {
                this.request_queue.Enqueue(new MessageRoundTrip(message));
            }
            else
            {
                this.message_queue.Enqueue(message);
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

            this.channel.NodeStatistics.MessageStatistics.AddTcpInMessage(msg);

            if (this.request_queue.TryPeek(out MessageRoundTrip round_trip))
            {
                if (round_trip != null && round_trip.Message.AnswerMessage == msg.GetType())
                {
                    this.request_queue.TryDequeue(out _);
                }
            }
        }

        public void Close()
        {
            this.send_message_flag = false;

            if (this.executor_handler != null && !this.executor_handler.IsCanceled)
            {
                this.executor_handler.Cancel();
                this.executor_handler = null;
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
