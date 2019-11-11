using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Mineral.Utils
{
    public class ScheduledExecutorService
    {
        public class ScheduledExecutorHandle
        {
            private int due_time = 0;
            private int period = 0;
            private bool is_canceled = false;
            private bool is_shutdown = false;

            public bool IsCanceled
            {
                get { return this.is_canceled; }
            }

            public bool IsShutdown
            {
                get { return this.is_shutdown; }
            }

            public ScheduledExecutorHandle(int due_time, int period)
            {
                this.due_time = due_time;
                this.period = period;
            }

            public void Process(object handle)
            {
                if (handle is Action)
                {
                    Action func = handle as Action;
                    Stopwatch stop_watch = new Stopwatch();
                    long start = 0;
                    long duration = 0;

                    Thread.Sleep(this.due_time);

                    while (!is_canceled)
                    {
                        start = stop_watch.ElapsedMilliseconds;
                        func();
                        duration = stop_watch.ElapsedMilliseconds - start;

                        Thread.Sleep((int)(Math.Max(0, (this.period - duration))));
                    }
                }
                else if (handle is IRunnable)
                {
                    IRunnable cmd = handle as IRunnable;
                    Stopwatch stop_watch = new Stopwatch();
                    long start = 0;
                    long duration = 0;

                    Thread.Sleep(this.due_time);

                    while (!is_canceled)
                    {
                        start = stop_watch.ElapsedMilliseconds;
                        cmd.Run();
                        duration = stop_watch.ElapsedMilliseconds - start;

                        Thread.Sleep((int)(Math.Max(0, (this.period - duration))));
                    }
                }
            }

            public void Cancel()
            {
                this.is_canceled = true;
            }

            public void Shutdown()
            {
                Cancel();
                this.is_shutdown = true;
            }
        }

        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static ScheduledExecutorHandle Scheduled(Action action, int due_time, int period)
        {
            ScheduledExecutorHandle handle = new ScheduledExecutorHandle(due_time, period);
            new Thread(handle.Process).Start(action);

            return handle;
        }

        public static ScheduledExecutorHandle Scheduled(IRunnable command, int due_time, int period)
        {
            ScheduledExecutorHandle handle = new ScheduledExecutorHandle(due_time, period);
            new Thread(handle.Process).Start(command);

            return handle;
        }
        #endregion
    }
}
