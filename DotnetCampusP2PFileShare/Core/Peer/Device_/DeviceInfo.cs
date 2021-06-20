namespace DotnetCampusP2PFileShare.Core.Peer
{
    /// <summary>
    /// 只读设备，用于表示当前设备
    /// </summary>
    public interface IReadOnlyDeviceInfo
    {
        /// <summary>
        /// 程序版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 设备名
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// 设备id号
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// 设备端口
        /// </summary>
        string DevicePort { set; get; }
    }

    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceInfo : IReadOnlyDeviceInfo
    {
        public DeviceInfo()
        {
        }

        /// <inheritdoc />
        public DeviceInfo(string version, string deviceName, string deviceId)
        {
            Version = version;
            DeviceName = deviceName;
            DeviceId = deviceId;
        }

        public static DeviceInfo CreateDeviceInfo(DeviceConfiguration deviceConfiguration)
        {
            return new DeviceInfo(DeviceConfiguration.Version, deviceConfiguration.DeviceName,
                deviceConfiguration.DeviceId);
        }

        /// <summary>
        /// 程序版本
        /// </summary>
        public string Version { set; get; }

        /// <summary>
        /// 设备名
        /// </summary>
        public string DeviceName { set; get; }

        /// <summary>
        /// 设备id号
        /// </summary>
        public string DeviceId { set; get; }

        /// <summary>
        /// 设备端口
        /// </summary>
        public string DevicePort { set; get; }
    }
}