using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using DotnetCampusP2PFileShare.Core.Context;

namespace DotnetCampusP2PFileShare.Core
{
    public static class PortPicker
    {
        /// <summary>
        /// 当我们要创建一个Tcp/UDP Server connection ,我们需要一个范围在1024到65535之间的端口，此方法返回最小从5125开始的可用端口
        /// </summary>
        /// <returns>可用的端口</returns>
        public static int GetNextAvailablePort()
        {
            const int port = Const.DefaultPort;
            var ports = GetPortsInUse();
            if (ports.All(p => p != port))
            {
                return port;
            }

            return GetAvailablePort(IPAddress.Any);
        }

        private static int GetAvailablePort(IPAddress ip)
        {
            TcpListener listener = new TcpListener(ip, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// 获取所有在使用中的端口号
        /// </summary>
        /// <returns></returns>
        private static List<int> GetPortsInUse()
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = ipGlobalProperties.GetActiveTcpListeners();
            var udpEndPoints = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            var ports = new List<int>();
            ports.AddRange(tcpEndPoints.Select(p => p.Port));
            ports.AddRange(udpEndPoints.Select(p => p.Port));
            ports.AddRange(tcpConnInfoArray.Select(p => p.LocalEndPoint.Port));
            return ports;
        }
    }
}