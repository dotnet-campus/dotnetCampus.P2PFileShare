using DotnetCampusP2PFileShare.Core.Context;
using DotnetCampusP2PFileShare.SDK.Download;
using DotnetCampusP2PFileShare.SDK.Install;
using DotnetCampusP2PFileShare.SDK.Upload;

namespace DotnetCampusP2PFileShare.SDK
{
    public class P2PProvider
    {
        /// <inheritdoc />
        public P2PProvider(P2PSettings p2PSettings = null)
        {
            P2PSettings = p2PSettings ?? new P2PSettings();

            var p2PProcess = new P2PProcess(this);
            P2PProcess = p2PProcess;

            P2PDownloader = new P2PDownloader(this);

            P2PRegister = new P2PRegister(this);
        }

        /// <summary>
        /// 表示P2P默认的链接
        /// http://127.0.0.1:5125/
        /// </summary>
        public static string P2PHost => $"http://127.0.0.1:{Const.DefaultPort}/";

        internal P2PProcess P2PProcess { get; }

        /// <summary>
        /// 提供资源下载
        /// </summary>
        public P2PDownloader P2PDownloader { get; }

        /// <summary>
        /// 注册资源
        /// </summary>
        public P2PRegister P2PRegister { get; }


        public P2PSettings P2PSettings { get; }

        /// <summary>
        /// 尝试启动P2P服务，如果已经启动，那么忽略
        /// </summary>
        public bool TryActive()
        {
            return P2PProcess.TryStart();
        }
    }
}