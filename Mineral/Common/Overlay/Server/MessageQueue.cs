using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DotNetty.Transport.Channels;

namespace Mineral.Common.Overlay.Server
{
    using Message = Mineral.Common.Overlay.Message.Message;

    public class MessageQueue
    {
        #region Field
        private volatile bool send_message_flag = false;
        private readonly long send_time = 0;
        private Thread send_message_thread = null;
        private Channel channel = null;
        private IChannelHandlerContext context;
        private Queue<MessageRoundTrip> request_queue = new Queue<MessageRoundTrip>();
        private ConcurrentQueue<Message> message_queue = new ConcurrentQueue<Message>();

        private Timer task_timer = null;
        private Stopwatch stopwatch = Stopwatch.StartNew();
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Send()
        {
            MessageRoundTrip message_round_trip = request_queue.Peek();
            if (!this.send_message_flag || message_round_trip == null)
                return;

            if (message_round_trip.RetryTime > 0 && !message_round_trip.HasToRetry())
                return;

            if (message_round_trip.RetryTime > 0)
            {
                channel
            }
        }
        #endregion


        #region External Method
        public void Activate(IChannelHandlerContext context)
        {
            this.context = context;
            this.send_message_flag = true;

            this.task_timer = new Timer((object state) =>
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
                this.task_timer.Change(10, 0);
            }, this, 10, 0);
        }
        #endregion
    }
}
