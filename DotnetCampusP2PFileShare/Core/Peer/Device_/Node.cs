using System;
using Newtonsoft.Json;
using DotnetCampusP2PFileShare.Model;

namespace DotnetCampusP2PFileShare.Core.Peer
{
    /// <summary>
    /// 表示一个设备的点
    /// </summary>
    public class Node
    {
        /// <inheritdoc />
        public Node(string url)
        {
            Url = url;
            //IsLocal = isLocal;
        }

        /// <summary>
        /// 设备链接 http://172.18.134.32:5125/ 
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// 最近更新设备的时间
        /// </summary>
        public DateTime LastUpdate { set; get; }



        [JsonConverter(typeof(ConcreteTypeConverter<DeviceInfo>))]
        public IReadOnlyDeviceInfo DeviceInfo { set; get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (DeviceInfo is null)
            {
                return Url;
            }

            return $"DeviceName={DeviceInfo.DeviceName} Url={Url}";
        }
    }
}