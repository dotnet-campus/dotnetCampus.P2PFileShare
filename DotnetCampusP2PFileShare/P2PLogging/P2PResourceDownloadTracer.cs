using System.IO;

namespace DotnetCampusP2PFileShare.P2PLogging
{
    /// <summary>
    /// 追踪资源下载
    /// </summary>
    /// <remarks>
    /// 在 P2P 资源下载包括以下步骤
    /// - 收到下载清单
    /// - 嗅探资源
    ///  + 可能嗅探不到
    /// - 下载资源
    ///  + 可能无法下载
    ///  + 可能无法访问文件
    ///  + 可能下载完成无法移动文件
    /// </remarks>
    public class P2PResourceDownloadTracer
    {
        /// <summary>
        /// 追踪资源下载
        /// </summary>
        /// <param name="resourceId"></param>
        public P2PResourceDownloadTracer(string resourceId)
        {
            ResourceId = resourceId;
        }

        public string ResourceId { get; }

        /// <summary>
        /// 资源下载请求，从客户端发送下载清单
        /// </summary>
        public void DownloadRequest()
        {
            TraceStopwatch.Start(ResourceDownload);
            Info("开始下载资源");
        }

        /// <summary>
        /// 开始嗅探资源
        /// </summary>
        public void StartSniff()
        {
            Info("开始嗅探资源");
            TraceStopwatch.Start(Sniff);
        }

        /// <summary>
        /// 嗅探到资源
        /// </summary>
        public void FirstSniffResource()
        {
            var ms = TraceStopwatch.End(Sniff);

            var message = $"嗅探到资源{ResourceId}耗时{ms}ms";
            if (ms > 0)
            {
                P2PTracer.Report(message, EventId.FirstFindResource, forthType: ms.ToString());
            }
        }

        /// <summary>
        /// 追踪信息
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            P2PTracer.Info($"{message} id={ResourceId}", Tag);
        }

        /// <summary>
        /// 追踪调试信息
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            P2PTracer.Debug(message, Tag);
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="file"></param>
        public void SetFinished(FileInfo file)
        {
            Info("下载完成");
            var ms = TraceStopwatch.End(ResourceDownload);
            var filePath = file.FullName;
            var fileSize = file.Length;

            var speed = $"{fileSize / 1024.0 / 1024.0 / ms * 1000:0.00}MB/s";

            var message = $"下载完成，平均速度 {speed} 总时间 {ms}ms 文件大小 {fileSize} 文件路径 {filePath}";

            var description = $"下载{file.Name}完成，平均速度 {speed}";

            P2PTracer.Report(message, EventId.ResourceDownloadFinished, fileSize.ToString(), ms.ToString(),
                description);
        }

        /// <summary>
        /// 下载失败
        /// </summary>
        /// <param name="remark"></param>
        public void SetFail(string remark)
        {
            Info($"下载失败 {remark}");
            TraceStopwatch.End(ResourceDownload);
            // 不上报下载失败的，因为现在P2P还没部署，基本上资源都是下载失败的
        }

        private TraceStopwatch TraceStopwatch { get; } = new TraceStopwatch();
        private const string ResourceDownload = nameof(ResourceDownload);
        private const string Sniff = nameof(Sniff);

        private const string Tag = "P2PResourceDownload";
    }
}