using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.SDK.Install
{
    /// <summary>
    /// 用于判断P2P是否有权限安装
    /// </summary>
    public interface IP2PInstallAuthority
    {
        /// <summary>
        /// 判断是否有权限
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckAuthorityAsync();
    }
}