using System;
using System.IO;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Net
{
    public class ProcessToken
    {
        /// <inheritdoc />
        public ProcessToken()
        {
            Id = Guid.NewGuid().ToString("N");
            ProcessReporter = new ProcessReporter(Id);
        }

        public ProcessReporter ProcessReporter { get; }

        public P2PResourceDownloadTracer DownloadTracer { set; get; }

        public string Id { get; }

        public void SetFinished(FileInfo file)
        {
            ProcessReporter.SetProcess(ProcessReporter.MaxProcess, "下载完成");
            DownloadTracer.SetFinished(file);
        }

        public void SetProcess(double process, string remark = "")
        {
            ProcessReporter.SetProcess(process, remark);
            if (!string.IsNullOrEmpty(remark))
            {
                DownloadTracer.Debug(remark);
            }
        }

        public void SetFail(string remark = "")
        {
            ProcessReporter.SetProcess(-1, remark);
            DownloadTracer.SetFail(remark);
        }
    }

    public class ProcessReporter
    {
        public ProcessReporter(string id)
        {
            Id = id;
            LastUpdateTime = DateTime.Now;
        }

        public string Id { get; }

        public DateTime LastUpdateTime { get; private set; }

        public double Process { get; private set; }

        public double MaxProcess { set; get; } = 100;

        public string Remark { get; private set; }

        public void SetProcess(double process, string remark = "")
        {
            Process = process;
            Remark = remark;
            LastUpdateTime = DateTime.Now;
        }
    }
}