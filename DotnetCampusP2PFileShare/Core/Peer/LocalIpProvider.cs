using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DotnetCampusP2PFileShare.Core.Peer
{
    class LocalIpProvider
    {
        /// <inheritdoc />
        public LocalIpProvider()
        {
            LocalIpList = new Lazy<List<string>>(() =>
            {
                var localIpList = new List<string>();
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (IsLocalNet(ip))
                        {
                            localIpList.Add(ip.ToString());
                        }
                    }
                }

                return localIpList;
            });
        }

        private bool IsLocalNet(IPAddress ip)
        {
            var current = ConvertIp(ip.ToString());

            foreach (var (min, max) in new (string min, string max)[]
            {
                ("10.0.0.0", "10.255.255.255"),
                ("172.16.0.0", "172.31.255.255"),
                ("192.168.0.0", "192.168.255.255"),
            })
            {
                var minIp = ConvertIp(min);
                var maxIp = ConvertIp(max);

                if (current > minIp)
                {
                    if (current < maxIp)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static UInt32 ConvertIp(string ip)
        {
            var ipByteList = IPAddress.Parse(ip).GetAddressBytes();
            return BitConverter.ToUInt32(ipByteList.Reverse().ToArray());
        }


        private Lazy<List<string>> LocalIpList { get; }

        public bool IsLocal(string ip)
        {
            return GetLocalIpList().Contains(ip);
        }

        /// <summary>
        /// 获取本地 IP 地址
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<string> GetLocalIpList()
        {
            return LocalIpList.Value;
        }
    }
}