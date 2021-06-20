namespace DotnetCampusP2PFileShare.P2PLogging
{
    public static class EventId
    {
        /// <summary>
        /// 资源下载完成
        /// </summary>
        public const string ResourceDownloadFinished = "ResourceDownloadFinished";

        /// <summary>
        /// 第一次寻找到资源
        /// </summary>
        public const string FirstFindResource = "FirstFindResource";

        /// <summary>
        /// 连接到的设备，用于知道当前的P2P运行如何
        /// </summary>
        /// 如果局域网有100设备，但是P2P只能链接 10 设备，那么就需要改算法
        public const string DotnetCampusP2PFileShareConnectNode = "DotnetCampusP2PFileShareConnectNode";

        /// <summary>
        /// 启动
        /// </summary>
        public const string DotnetCampusP2PFileShareStartUp = "DotnetCampusP2PFileShareStartUp";

        /// <summary>
        /// 没有捕获异常
        /// </summary>
        public const string DotnetCampusP2PFileShareUnhandledException = "DotnetCampusP2PFileShareUnhandledException";

        /// <summary>
        /// 下载异常
        /// </summary>
        public const string DotnetCampusP2PFileShareDownloadException = "DotnetCampusP2PFileShareDownloadException";

        public const string DotnetCampusP2PFileShareUploadResource = "DotnetCampusP2PFileShareUploadResource";
    }
}