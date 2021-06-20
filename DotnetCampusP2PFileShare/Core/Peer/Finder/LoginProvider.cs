using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotnetCampusP2PFileShare.Core.Context;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;
using HttpClientExtensions = DotnetCampusP2PFileShare.Core.Net.HttpClientExtensions;

namespace DotnetCampusP2PFileShare.Core.Peer.Finder
{
    /// <summary>
    /// 提供注册到服务器
    /// </summary>
    public class LoginProvider
    {
        /// <inheritdoc />
        public LoginProvider(PeerFinder peerFinder)
        {
            PeerFinder = peerFinder;

            CurrentDeviceContent =
                new StringContent(peerFinder.CurrentDeviceInfoJson, Encoding.UTF8, "application/json");

            Task.Run(async () =>
            {
                while (true)
                {
                    var (ip, port) = await AsyncQueue.DequeueAsync();
                    await TryLoginAsync(ip, port);
                }
            });
        }

        public void RegisterLoginTask(string ip, string port)
        {
            AsyncQueue.Enqueue((ip, port));
        }

        /// <summary>
        /// 尝试登陆到服务器，登陆到服务器可以连接到服务器，可以拿到其他和服务器连接的设备拿到本机主IP是哪个
        /// </summary>
        public async Task<(bool successed, LoginInfo loginInfo)> TryLoginAsync(string ip, string port)
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

            LoginInfo loginInfo = null;
            if (PeerFinder.CheckIsKnownNode(ip))
            {
                Log($"因为{ip}属于已知ip所以跳过");
                return (false, loginInfo);
            }

            if (RecentVisitIPList.TryGetValue(ip, out var time))
            {
                // 如果最近1分钟有访问过，那么就跳过
                if (DateTime.Now - time < TimeSpan.FromMinutes(1))
                {
                    Log($"因为最近1分钟访问过{ip}所以跳过");
                    return (false, loginInfo);
                }
            }

            RecentVisitIPList.AddOrUpdate(ip, DateTime.Now, (s, dateTime) => DateTime.Now);
            CleanRecentVisitIPList();

            var url = $"http://{ip}:{port}/";
            var login = $"{url}api/Peer/Login";

            try
            {
                var stringContent = CurrentDeviceContent;

                using var message = await httpClient.PostAsync(login,
                    new StringContent(PeerFinder.CurrentDeviceInfoJson, Encoding.UTF8, "application/json"));
                if (message.StatusCode == HttpStatusCode.OK)
                {
                    loginInfo = await HttpClientExtensions.ReadAsAsync<LoginInfo>(message.Content);

                    var deviceInfo = loginInfo.DeviceInfo;

                    Log($"++ 连接{url}成功，勾搭了 {deviceInfo.DeviceName}");
                    var node = new Node(url)
                    {
                        DeviceInfo = deviceInfo,
                        LastUpdate = DateTime.Now
                    };
                    PeerFinder.AddOrUpdateKnownNode(ip, node);

                    RegisterLoginTask(loginInfo.Friends);

                    return (true, loginInfo);
                }
            }
            catch (HttpRequestException e)
            {
                if
                (
                    e.InnerException is SocketException socketException &&
                    (
                        socketException.ErrorCode == 10061 // 由于对方设备积极拒绝
                        || socketException.ErrorCode == 10049 // 在其上下文中，该请求的地址无效
                    )
                )
                {
                    // 由于对方设备积极拒绝
                    return (false, loginInfo);
                }

                Log($"连接{url}失败");
            }
            catch (TaskCanceledException)
            {
                // 超时，对方设备不存在
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }

            return (false, loginInfo);
        }

        private readonly object _locker = new object();

        private bool _startClean;

        private StringContent CurrentDeviceContent { get; }

        /// <summary>
        /// 最近访问的地址，减少重复访问
        /// </summary>
        private ConcurrentDictionary<string, DateTime> RecentVisitIPList { get; } =
            new ConcurrentDictionary<string, DateTime>();

        private void CleanRecentVisitIPList()
        {
            if (_startClean) return;
            lock (_locker)
            {
                if (_startClean) return;

                _startClean = true;
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(10));

                    foreach (var key in RecentVisitIPList.Keys)
                    {
                        if (RecentVisitIPList.TryGetValue(key, out var time))
                        {
                            if (DateTime.Now - time > TimeSpan.FromMinutes(10))
                            {
                                RecentVisitIPList.TryRemove(key, out _);
                            }
                        }
                    }

                    _startClean = false;

                    if (RecentVisitIPList.Count > 0)
                    {
                        CleanRecentVisitIPList();
                    }
                });
            }
        }

        public void RegisterLoginTask(List<Node> loginInfoFriends)
        {
            foreach (var loginInfoFriend in loginInfoFriends)
            {
                AsyncQueue.Enqueue(loginInfoFriend.GetIpFromNode());
            }
        }

        private AsyncQueue<(string ip, string port)> AsyncQueue { get; } = new AsyncQueue<(string ip, string port)>();

        private PeerFinder PeerFinder { get; }

        private void Log(string message)
        {
            P2PTracer.Info(message, "登陆服务器");
        }
    }
}