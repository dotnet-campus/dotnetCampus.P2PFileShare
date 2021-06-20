using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Peer.Finder
{
    /// <summary>
    /// 中央服务器发现
    /// </summary>
    class TracerFinder
    {
        /// <inheritdoc />
        public TracerFinder(PeerFinder peerFinder)
        {
            PeerFinder = peerFinder;
        }

        public async void FindPeer()
        {
            const string tracerTag = "TracerFinder";
            await Task.Delay(TimeSpan.FromMinutes(10));

            if (PeerFinder.GetCurrentNode().Any())
            {
                // 不急着去注册
                await Task.Delay(TimeSpan.FromMinutes(20));
            }

            while (true)
            {
                bool success = false;
                try
                {
                    P2PTracer.Info($"中央服务器发现开始执行", tracerTag);

                    var port = AppConfiguration.Current.CurrentDeviceInfo.DevicePort;

                    var localIp = string.Join(';',
                        PeerFinder.LocalIpProvider.GetLocalIpList().Select(temp => $"{temp}:{port}"));

                    var url = $"http://p2p.api.acmx.xyz/api/peer/{localIp}";

                    var httpClient = new HttpClient()
                    {
                        Timeout = TimeSpan.FromMinutes(10)
                    };

                    using (httpClient)
                    {
                        var remoteIp = await httpClient.GetStringAsync(url);
                        P2PTracer.Info($"中央服务器返回 {remoteIp}", tracerTag);
                        var ipList = GetIpList(remoteIp).Where(temp =>
                            !string.IsNullOrEmpty(temp.ip) && !string.IsNullOrEmpty(temp.port)).ToList();
                        P2PTracer.Info($"从服务器找到{ipList.Count}设备", tracerTag);

                        RegisterLoginTask(ipList);
                    }

                    success = true;
                }
                catch (Exception e)
                {
                    P2PTracer.Info(e.ToString(), tracerTag);
                }

                if (success)
                {
                    await Task.Delay(TimeSpan.FromHours(1));
                }
                else
                {
                    Random ran = new Random();
                    await Task.Delay(TimeSpan.FromMinutes(ran.Next(5, 30)));
                }
            }
        }

        private void RegisterLoginTask(List<(string ip, string port)> ipList)
        {
            foreach (var (ip, port) in ipList)
            {
                PeerFinder.LoginProvider.RegisterLoginTask(ip, port);
            }
        }

        private IEnumerable<(string ip, string port)> GetIpList(string remoteIp)
        {
            if (string.IsNullOrEmpty(remoteIp))
            {
                yield break;
            }

            var ipList = remoteIp.Split(';');
            foreach (var ip in ipList)
            {
                yield return IpRegex.Parse(ip);
            }
        }


        private PeerFinder PeerFinder { get; }
    }
}