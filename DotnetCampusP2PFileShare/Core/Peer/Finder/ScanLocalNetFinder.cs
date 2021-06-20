using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using DotnetCampusP2PFileShare.Core.Context;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Peer.Finder
{

    /// <summary>
    /// 局域网网段扫描
    /// </summary>
    public class ScanLocalNetSegmentFinder
    {
        /// <inheritdoc />
        public ScanLocalNetSegmentFinder(PeerFinder peerFinder)
        {
            PeerFinder = peerFinder;
        }

        /// <summary>
        /// 局域网扫描
        /// </summary>
        public async void ScanLocalNet()
        {
            // 网段扫描，一个本机存在多个地址
            // 多个地址之间都需要扫描网段
            var taskList = new List<Task>();
            foreach (var ipAddress in PeerFinder.LocalIpProvider.GetLocalIpList())
            {
                // 获取本机的 IP 地址
                var ip = ipAddress;

                // 取本机地址作为网段，如本机是 172.19.169.13 那么扫描 172.19.169 网段，从 1 到 254 这一段
                var ipNet = GetIpNet(ip);

                taskList.Add(ScanNet(ipNet));
            }

            await Task.WhenAny(taskList).ConfigureAwait(false);

            ScanMainList();

            await Task.WhenAll(taskList);

            foreach (var ip in PeerFinder.LocalIpProvider.GetLocalIpList())
            {
                var localNetSegmentProvider = new LocalNetSegmentProvider(ip);
                if (MainLocalNetSegmentProviderList.Contains(localNetSegmentProvider))
                {
                    continue;
                }

                if (LocalNetSegmentProviderList.Contains(localNetSegmentProvider))
                {
                    continue;
                }

                LocalNetSegmentProviderList.Add(localNetSegmentProvider);
            }

            RunScanLocalNet();
        }

        private void ScanMainList()
        {
            if (MainIpList.Count > 0)
            {
                foreach (var mainIp in MainIpList)
                {
                    var localNetSegmentProvider = new LocalNetSegmentProvider(mainIp);
                    if (!MainLocalNetSegmentProviderList.Contains(localNetSegmentProvider))
                    {
                        MainLocalNetSegmentProviderList.Add(localNetSegmentProvider);
                    }
                }

                RunScanLocalNet();
            }
        }

        private void RunScanLocalNet()
        {
            if (_scanning) return;
            lock (MainLocalNetSegmentProviderList)
            {
                if (_scanning) return;
                _scanning = true;
            }

            Task.Run(async () =>
            {
                var random = new Random();

                while (MainLocalNetSegmentProviderList.Count > 0 || LocalNetSegmentProviderList.Count > 0)
                {
                    try
                    {
                        ConcurrentBag<LocalNetSegmentProvider>
                            localNetSegmentProviderList = MainLocalNetSegmentProviderList;

                        if (MainLocalNetSegmentProviderList.Count == 0)
                        {
                            localNetSegmentProviderList = LocalNetSegmentProviderList;
                        }
                        else if (LocalNetSegmentProviderList.Count > 0)
                        {
                            if (random.Next(2) == 1)
                            {
                                localNetSegmentProviderList = LocalNetSegmentProviderList;
                            }
                        }

                        var ip = GetRandomIp(localNetSegmentProviderList, random);
                        var ipNet = GetIpNet(ip);
                        if (ScanningIpList.Contains(ipNet))
                        {
                            continue;
                        }

                        var defaultPort = Const.DefaultPort.ToString();
                        var (successed, loginInfo) = await PeerFinder.LoginProvider.TryLoginAsync($"{ip}", defaultPort);
                        if (successed)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(10));
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                    }
                    catch (Exception e)
                    {
                        P2PTracer.Info(e.ToString(), "RunScanLocalNet");
                    }
                }

                _scanning = false;
            });
        }

        private string GetRandomIp(ConcurrentBag<LocalNetSegmentProvider> localNetSegmentProviderList, Random random)
        {
            var n = random.Next(localNetSegmentProviderList.Count);
            var localNetSegmentProvider = localNetSegmentProviderList.ElementAtOrDefault(n);

            while (localNetSegmentProvider is null)
            {
                n = random.Next(localNetSegmentProviderList.Count);
                localNetSegmentProvider = localNetSegmentProviderList.ElementAtOrDefault(n);
            }

            var ipAddress = localNetSegmentProvider.GetIp();
            return ipAddress.ToString();
        }

        private bool _scanning;

        public void AddKnownIp(string ip)
        {
        }

        /// <summary>
        /// 获取网段
        /// </summary>
        /// <param name="ip"></param>
        private string GetIpNet(string ip)
        {
            //如本机是 172.19.169.13 那么扫描 172.19.169 网段
            var n = ip.LastIndexOf('.');
            ip = ip.Substring(0, n);
            return ip;
        }

        private async Task ScanNet(string ip)
        {
            ScanningIpList.Add(ip);

            await Task.Run(async () =>
            {
                for (var i = 0; i < 255; i++)
                {
                    if (MainIpList.Count > 0)
                    {
                        if (!MainIpList.Any(temp => temp.Contains(ip)))
                        {
                            // 当前IP不属于主IP就降低访问速度
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                    }

                    var defaultPort = Const.DefaultPort.ToString();
                    var (successed, loginInfo) = await PeerFinder.LoginProvider.TryLoginAsync($"{ip}.{i}", defaultPort);

                    if (successed)
                    {
                        if (MainIpList.All(temp => temp != loginInfo.MainIp))
                        {
                            MainIpList.Add(loginInfo.MainIp);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        private ConcurrentBag<LocalNetSegmentProvider> MainLocalNetSegmentProviderList { get; } =
            new ConcurrentBag<LocalNetSegmentProvider>();

        private ConcurrentBag<LocalNetSegmentProvider> LocalNetSegmentProviderList { get; } =
            new ConcurrentBag<LocalNetSegmentProvider>();


        private ConcurrentBag<string> ScanningIpList { get; } = new ConcurrentBag<string>();

        private ConcurrentBag<string> MainIpList { get; } = new ConcurrentBag<string>();

        /// <summary>
        /// 获取本地 IP 地址
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPAddress> GetLocalIpList()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    yield return ip;
                }
            }
        }

        private PeerFinder PeerFinder { get; }
    }
}