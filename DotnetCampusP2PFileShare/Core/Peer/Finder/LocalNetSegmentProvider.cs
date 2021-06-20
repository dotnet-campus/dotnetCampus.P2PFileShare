using System;
using System.Linq;
using System.Net;

namespace DotnetCampusP2PFileShare.Core.Peer.Finder
{
    /// <summary>
    /// 提供局域网网段
    /// </summary>
    class LocalNetSegmentProvider
    {
        /// <inheritdoc />
        public LocalNetSegmentProvider(string ip)
        {
            Ip = ip;

            if (ip.Contains("10."))
            {
                IpNext = ConvertIp("10.0.0.1");
                IpEnd = ConvertIp("10.255.255.255");
                //IpNext = IPAddress.Parse("10.0.0.0").GetAddressBytes();
                //IpEnd = IPAddress.Parse("10.255.255.255").GetAddressBytes();
                Random = new Random();
            }
            else if (ip.Contains("172."))
            {
                IpNext = ConvertIp("172.16.0.1");
                IpEnd = ConvertIp("172.31.255.255");
                //IpNext = IPAddress.Parse("172.16.0.0").GetAddressBytes();
                //IpEnd = IPAddress.Parse("172.31.255.255").GetAddressBytes();
                Random = new Random();
            }
            else if (ip.Contains("192.168."))
            {
                IpNext = ConvertIp("192.168.0.2");
                IpEnd = ConvertIp("192.168.255.255");
            }
        }

        private static UInt32 ConvertIp(string ip)
        {
            var ipByteList = IPAddress.Parse(ip).GetAddressBytes();
            return BitConverter.ToUInt32(ipByteList.Reverse().ToArray());
        }

        private static IPAddress ConvertToIp(uint ip)
        {
            var ipByteList = BitConverter.GetBytes(ip).Reverse().ToArray();
            return new IPAddress(ipByteList);
        }

        private UInt32 IpEnd { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                return Equals((LocalNetSegmentProvider)obj);
            }

            return false;
        }

        private bool Equals(LocalNetSegmentProvider other)
        {
            return IpEnd == other.IpEnd;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)IpEnd;
        }

        private UInt32 IpNext { set; get; }

        public string Ip { get; }

        private Random Random { get; }

        public IPAddress GetIp()
        {
            if (Random == null)
            {
                var ipAddress = ConvertToIp(IpNext);
                IpNext++;

                if (IpNext == IpEnd)
                {
                    IpNext = ConvertIp("192.168.0.2");
                }

                return ipAddress;
            }
            else
            {
                var ip = (uint)((IpEnd - IpNext) * Random.NextDouble()) + IpNext;
                var ipAddress = ConvertToIp(ip);
                return ipAddress;
            }
        }
    }
}