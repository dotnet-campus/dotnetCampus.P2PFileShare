using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetCampusP2PFileShare.Core.Context;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Peer.Finder
{
    /// <summary>
    /// 组播方式找到设备
    /// </summary>
    internal class PeerMulticastFinder : IDisposable
    {
        /// <inheritdoc />
        public PeerMulticastFinder()
        {
            MulticastSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
            MulticastAddress = IPAddress.Parse("230.139.69.2");
        }

        /// <summary>
        /// 组播地址
        /// <para/>
        /// 224.0.0.0～224.0.0.255为预留的组播地址（永久组地址），地址224.0.0.0保留不做分配，其它地址供路由协议使用；
        /// <para/>
        /// 224.0.1.0～224.0.1.255是公用组播地址，可以用于Internet；
        /// <para/>
        /// 224.0.2.0～238.255.255.255为用户可用的组播地址（临时组地址），全网范围内有效；
        /// <para/>
        /// 239.0.0.0～239.255.255.255为本地管理组播地址，仅在特定的本地范围内有效。
        /// </summary>
        public IPAddress MulticastAddress { set; get; }

        public int MulticastPort { set; get; }

        public IPAddress LocalIpAddress { set; get; } = IPAddress.Any;

        /// <summary>
        /// 收到消息
        /// </summary>
        public event EventHandler<string> ReceivedMessage;

        /// <summary>
        /// 寻找局域网设备
        /// </summary>
        public static void FindPeer(PeerFinder peerFinder)
        {
            var ipList = peerFinder.LocalIpProvider.GetLocalIpList();
            foreach (var ipAddress in ipList)
            {
                var peerMulticastFinder = new PeerMulticastFinder()
                {
                    LocalIpAddress = IPAddress.Parse(ipAddress),
                    // 虽然TCP和UDP可以使用相同端口
#if DEBUG
                    MulticastPort = 11006
#else
                    MulticastPort = Const.DefaultPort + 5
#endif
                };

                // 实际是反过来，让其他设备询问
                peerMulticastFinder.StartMulticast();
                var message = $"{ipAddress}:{AppConfiguration.Current.CurrentDeviceInfo.DevicePort}";

                var random = new Random();

                peerMulticastFinder.SendBroadcastMessage(message);
                // 先发送再获取消息，这样就不会收到自己发送的消息
                peerMulticastFinder.ReceivedMessage += async (s, e) =>
                {
                    try
                    {
                        var multicastFinder = (PeerMulticastFinder) s;

                        var (ip, port) = IpRegex.Parse(e);

                        if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
                        {
                            if (peerFinder.LocalIpProvider.IsLocal(ip))
                            {
                                return;
                            }

                            var delay = random.Next(100, 60000);

                            await Task.Delay(delay);

                            var (successed, loginInfo) = await peerFinder.LoginProvider.TryLoginAsync(ip, port);
                            if (successed)
                            {
                                var syncFriend = peerFinder.SyncFriend(loginInfo.Friends);
                                if (syncFriend.Any())
                                {
                                    using var httpClient = new HttpClient();
                                    var url = $"http://{ip}:{port}/api/Peer/SyncFriend";
                                    await httpClient.PostAsJsonAsync(url, syncFriend);
                                }
                            }
                        }
                        else
                        {
#if DEBUG
                            Debugger.Break();
#endif

                            // 其他捣乱的应用
                            P2PTracer.Info(
                                $"报告 发现有捣乱的人在 {multicastFinder.MulticastAddress}:{multicastFinder.MulticastPort} 上乱发消息，让我无法解析 {e} 作为地址");
                        }
                    }
                    catch (Exception exception)
                    {
                        P2PTracer.Report(exception, "组播接收消息");
                    }
                };
            }
        }

        /// <summary>
        /// 启动组播
        /// </summary>
        public void StartMulticast()
        {
            try
            {
                // 如果首次绑定失败，那么将无法接收，但是可以发送
                TryBindSocket();

                // Define a MulticastOption object specifying the multicast group 
                // address and the local IPAddress.
                // The multicast group address is the same as the address used by the server.
                // 有多个 IP 时，指定本机的 IP 地址，此时可以接收到具体的内容
                var multicastOption = new MulticastOption(MulticastAddress, LocalIpAddress);

                MulticastSocket.SetSocketOption(SocketOptionLevel.IP,
                    SocketOptionName.AddMembership,
                    multicastOption);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Task.Run(ReceiveBroadcastMessagesAsync);
        }

        /// <summary>
        /// 发送组播
        /// </summary>
        /// <param name="message"></param>
        public void SendBroadcastMessage(string message)
        {
            try
            {
                var endPoint = new IPEndPoint(MulticastAddress, MulticastPort);
                var byteList = Encoding.UTF8.GetBytes(message);

                if (byteList.Length > MaxByteLength)
                {
                    throw new ArgumentException($"传入 message 转换为 byte 数组长度太长，不能超过{MaxByteLength}字节")
                    {
                        Data =
                        {
                            { "message", message },
                            { "byteList", byteList }
                        }
                    };
                }

                MulticastSocket.SendTo(byteList, endPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e);
            }
        }

        private void ReceiveBroadcastMessagesAsync()
        {
            // 接收需要绑定 MulticastPort 端口
            var bytes = new byte[MaxByteLength];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (!_disposedValue)
                {
                    var length = MulticastSocket.ReceiveFrom(bytes, ref remoteEndPoint);

                    OnReceivedMessage(Encoding.UTF8.GetString(bytes, 0, length));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private Socket MulticastSocket { get; }

        private void TryBindSocket()
        {
            for (var i = MulticastPort; i < 65530; i++)
            {
                try
                {
                    EndPoint localEndPoint = new IPEndPoint(LocalIpAddress, i);
                    MulticastSocket.Bind(localEndPoint);

                    if (i != MulticastPort)
                    {
                        P2PTracer.Debug($"原本绑定 {MulticastPort} 使用{localEndPoint}访问");
                    }

                    return;
                }
                catch (SocketException e)
                {
                    //P2PTracer.Debug($"{localEndPoint} {e}");
                }
            }
        }

        private const int MaxByteLength = 1024;

        private void OnReceivedMessage(string e)
        {
            ReceivedMessage?.Invoke(this, e);
        }

        #region IDisposable Support

        private bool _disposedValue; // 要检测冗余调用

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                MulticastSocket.Dispose();

                ReceivedMessage = null;
                MulticastAddress = null;

                _disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}