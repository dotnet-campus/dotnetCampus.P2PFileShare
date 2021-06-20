using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.Core.Net
{
    public class ProcessManager
    {
        /// <inheritdoc />
        public ProcessManager()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var maxTime = TimeSpan.FromMinutes(10);
                    await Task.Delay(maxTime);
                    foreach (var key in ProcessReportList.Keys)
                    {
                        if (ProcessReportList.TryGetValue(key, out var processReport))
                        {
                            if (DateTime.Now - processReport.ProcessReporter.LastUpdateTime > maxTime)
                            {
                                Remove(key);
                            }
                        }
                    }
                }
            });
        }

        public void Register(ProcessToken processReport)
        {
            ProcessReportList.TryAdd(processReport.Id, processReport);
        }

        public bool TryGetProcessReport(string id, out ProcessToken processReport)
        {
            return ProcessReportList.TryGetValue(id, out processReport);
        }

        public void Remove(string id)
        {
            ProcessReportList.TryRemove(id, out _);
        }

        private ConcurrentDictionary<string, ProcessToken> ProcessReportList { get; } =
            new ConcurrentDictionary<string, ProcessToken>();
    }
}