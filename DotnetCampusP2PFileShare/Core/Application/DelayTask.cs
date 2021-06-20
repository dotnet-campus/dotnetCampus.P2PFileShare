using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Application
{
    public static class DelayTask
    {
        public static void AddTask(Action action, TimeSpan delay = default)
        {
            if (delay == default)
            {
                delay = TimeSpan.FromSeconds(5);
            }

            ActionList.Add((action, delay));
        }

        public static async void Run()
        {
            var minDelay = TimeSpan.MaxValue;
            (Action action, TimeSpan delay) firstTask = default;
            foreach (var temp in ActionList)
            {
                if (temp.delay < minDelay)
                {
                    minDelay = temp.delay;
                    firstTask = temp;
                }
            }

            ActionList.Remove(firstTask);

            await Task.Delay(minDelay);

            await Task.Run(async () =>
            {
                try
                {
                    firstTask.action();
                }
                catch (Exception)
                {
                    // 忽略
                }

                foreach (var (action, delay) in ActionList.OrderBy(temp => temp.delay))
                {
                    await Task.Delay(delay);

                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        // 忽略
                        P2PTracer.Info(e.ToString());
                    }
                }

                ActionList.Clear();
            });
        }

        private static List<(Action action, TimeSpan delay)> ActionList { get; } =
            new List<(Action action, TimeSpan delay)>();
    }
}