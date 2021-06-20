using System;
using System.Reflection;
using dotnetCampus.Configurations;

namespace DotnetCampusP2PFileShare.Core.Peer
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceConfiguration : Configuration
    {
        /// <inheritdoc />
        static DeviceConfiguration()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// 程序版本
        /// </summary>
        public static string Version { get; }

        /// <summary>
        /// 设备名
        /// </summary>
        public string DeviceName
        {
            set => SetValue(value);
            get
            {
                var deviceName = GetString();
                if (deviceName.HasValue)
                {
                    return deviceName;
                }

                deviceName = Environment.UserName;
                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    deviceName = Environment.MachineName;
                }

                DeviceName = deviceName;
                return deviceName;
            }
        }

        /// <summary>
        /// 设备id号
        /// </summary>
        public string DeviceId
        {
            set => SetValue(value);
            get
            {
                var deviceId = GetString();
                if (deviceId != null)
                {
                    return deviceId;
                }

                deviceId = Guid.NewGuid().ToString("N");
                DeviceId = deviceId;
                return deviceId;
            }
        }
    }
}