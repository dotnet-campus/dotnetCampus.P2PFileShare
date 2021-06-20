using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.SDK.Install
{
    /// <summary>
    /// 用于启动P2P软件
    /// </summary>
    public interface IP2PBoot
    {
        /// <summary>
        /// 启动P2P应用，如果启动不上了，那么将会尝试调用安装
        /// </summary>
        /// <returns></returns>
        bool StartP2P();

        /// <summary>
        /// 尝试安装P2P应用
        /// </summary>
        /// <returns></returns>
        Task<bool> TryInstallP2PFile();
    }
}