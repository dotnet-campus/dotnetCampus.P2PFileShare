using System;

namespace DotnetCampusP2PFileShare.SDK.Download
{
    /// <summary>
    /// 追踪P2P下载方式
    /// </summary>
    public class P2PDownloadTracer
    {
        /// <summary>
        /// 是否从P2P下载
        /// </summary>
        public bool DownloadFromP2P { set; get; }

        /// <summary>
        /// 下载所使用时间
        /// </summary>
        public TimeSpan CostTime { set; get; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { set; get; }
    }
}