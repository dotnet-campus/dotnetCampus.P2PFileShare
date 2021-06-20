using System.Collections.Generic;
using DotnetCampusP2PFileShare.Core.Peer;

namespace DotnetCampusP2PFileShare.Model
{
    /// <summary>
    /// 登录信息
    /// </summary>
    public class LoginInfo
    {
        /// <summary>
        /// 主要的 IP 是哪个，在访问其他设备的时候，其他设备返回访问的 IP 是哪个
        /// </summary>
        public string MainIp { get; set; }

        /// <summary>
        /// 服务器端的设备
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }

        public List<Node> Friends { get; set; }
    }
}