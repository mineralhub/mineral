using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mineral.Utils
{
    public static class Profiler
    {
        private static List<ProfilerFrame> stack = new List<ProfilerFrame>();
        private static Dictionary<string, ProfiledBlock> totals = new Dictionary<string, ProfiledBlock>();
        private static Action<string> logger;

        public static void SetLogger(Action<string> logger)
        {
            Profiler.logger = logger;
        }

        public static ProfilerFrame Measure(string name)
        {
            return new ProfilerFrame(name);
        }
        public static void PushFrame(ProfilerFrame frame)
        {
            if (logger != null)
            {
                var oneUp = stack.LastOrDefault();
                if (oneUp != null && !oneUp.IsFrameEntranceLogged)
                {
                    logger(new String(' ', stack.Count * 2 - 2) + " => " + oneUp.Name);
                    oneUp.IsFrameEntranceLogged = true;
                }
            }
            stack.Add(frame);
        }
        public static void PushFrame(string name)
        {
            Profiler.Measure(name);
        }
        public static void PopFrame(string name)
        {
            Profiler.PopFrame(stack[stack.Count - 1]);
        }
        public static void PopFrame()
        {
            Profiler.PopFrame(stack[stack.Count - 1]);
        }
        public static void PopFrame(ProfilerFrame frame)
        {
            if (logger != null)
            {
                logger(new String(' ', stack.Count * 2 - 2) + (frame.IsFrameEntranceLogged ? " <= " : " <> ") + frame.Name + ": " + frame.Stopwatch.ElapsedMilliseconds + "ms");
            }
            var total = totals.ContainsKey(frame.Name) ? totals[frame.Name] : new ProfiledBlock(frame.Name);
            total.Add(frame);
            totals[frame.Name] = total;
            stack.RemoveAt(stack.Count - 1);
        }
        public static void NextFrame(string name)
        {
            PopFrame();
            Measure(name);
        }

        public static IEnumerable<ProfiledBlock> Totals { get { return totals.Values; } }
    }

    public class ProfilerFrame : IDisposable
    {
        public string Name { get; private set; }
        public Stopwatch Stopwatch { get; private set; }
        public bool IsFrameEntranceLogged { get; set; }

        public ProfilerFrame(string name)
        {
            Name = name;
            Profiler.PushFrame(this);
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            Profiler.PopFrame(this);
        }
    }

    public class ProfiledBlock
    {
        public string Name { get; private set; }
        public long TotalExecutionTimeInMilliseconds { get; private set; }
        public long /*Moving*/AverageExecutionTimeInMilliseconds { get; private set; }
        public int TotalInvocationCount { get; private set; }

        public ProfiledBlock(string name)
        {
            Name = name;
        }

        public void Add(ProfilerFrame frame)
        {
            TotalExecutionTimeInMilliseconds += frame.Stopwatch.ElapsedMilliseconds;
            AverageExecutionTimeInMilliseconds = (long)Math.Round((9.0 * AverageExecutionTimeInMilliseconds + frame.Stopwatch.ElapsedMilliseconds) / 10.0);
            TotalInvocationCount++;
        }
    }
}
