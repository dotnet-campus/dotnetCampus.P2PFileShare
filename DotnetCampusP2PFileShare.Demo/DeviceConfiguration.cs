namespace DotnetCampusP2PFileShare.Core.Peer
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceConfiguration
    {
        /// <summary>
        /// 程序版本
        /// </summary>
        public static string Version { get; }

        /// <summary>
        /// 设备名
        /// </summary>
        public string DeviceName { set; get; }

        /// <summary>
        /// 设备id号
        /// </summary>
        public string DeviceId { set; get; }
    }
}