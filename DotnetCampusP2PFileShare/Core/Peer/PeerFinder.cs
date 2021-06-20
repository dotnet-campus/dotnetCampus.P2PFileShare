using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.Core.Peer.Finder;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Peer
{
    /// <summary>
    /// 寻找其他设备
    /// </summary>
    /// 这是一个程序集
    public class PeerFinder
    {
        public PeerFinder()
        {
            var currentDeviceInfo = AppConfiguration.Current.CurrentDeviceInfo;
            CurrentDeviceInfo = currentDeviceInfo;

            CurrentDeviceInfoJson = JsonConvert.SerializeObject(currentDeviceInfo);

            LoginProvider = new LoginProvider(this);
        }

        public LoginProvider LoginProvider { get; }

        /// <summary>
        /// 当前的设备
        /// </summary>
        public IReadOnlyDeviceInfo CurrentDeviceInfo { get; }

        public string CurrentDeviceInfoJson { get; }

        /// <summary>
        /// 寻找局域网内其他设备
        /// </summary>
        public void FindPeer()
        {
            // 第一步从文件读取上一次找到的
            var localFileKnownDeviceManager = new LocalFileKnownDeviceManager(KnownNodeList);
            PingLastKnownList(localFileKnownDeviceManager);

            // 尝试连接上之后询问当前的设备

            // 组播
            PeerMulticastFinder.FindPeer(this);

            // 从中央服务器询问局域网内的设备
            var tracerFinder = new TracerFinder(this);
            tracerFinder.FindPeer();

            var scanLocalNetSegmentFinder = new ScanLocalNetSegmentFinder(this);
            scanLocalNetSegmentFinder.ScanLocalNet();

            localFileKnownDeviceManager.AutoTaskToWriteKnownList();

            AutoBeat();
        }

        public bool CheckIsKnownNode(string ip)
        {
            return LocalIpProvider.IsLocal(ip) || KnownNodeList.ContainsKey(ip);
        }


        public List<Node> GetCurrentNode()
        {
            //RunTimeTest.AddTest(()=> AddOrUpdateKnownNode("123.123.12.12", new DeviceInfo()));

            return KnownNodeList.ToArray()
                // 注释下面代码，让获取资源可以从本地获取，用于测试
                // 可以从本地下载资源就不需要去找另一个设备
                //.Where(temp => !temp.Value.IsLocal)
                .Select(temp => temp.Value).ToList();
        }

        /// <summary>
        /// 关系维护
        /// </summary>
        private void AutoBeat()
        {
            Task.Run(async () =>
            {
                var minTime = TimeSpan.FromMinutes(10);
                while (true)
                {
                    await Task.Delay(minTime);

                    HttpClient httpClient = null;
                    foreach (var (key, node) in KnownNodeList.ToList().OrderBy(pair => pair.Value.LastUpdate).ToList())
                    {
                        if (DateTime.Now - node.LastUpdate < minTime)
                        {
                            break;
                        }

                        httpClient = httpClient ?? new HttpClient();

                        Log($"维护{node}关系");
                        await NodeSwap.SendMessage(node, "api/Peer/Beat", CurrentDeviceInfoJson, httpClient);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    httpClient?.Dispose();
                }
            });
        }

        /// <summary>
        /// 尝试连接上次设备
        /// </summary>
        /// <param name="localFileKnownDeviceManager"></param>
        private void PingLastKnownList(LocalFileKnownDeviceManager localFileKnownDeviceManager)
        {
            Task.Run(async () =>
            {
                var knownList = await localFileKnownDeviceManager.ReadFromFileAsync();

                if (knownList.Count == 0)
                {
                    Log("没有读取到已知的设备，将需要较长时间初始化");

                    // 测试代码
                    //knownList.Add(("172.18.134.16", "59091"));
                }

                foreach (var (ip, port) in knownList)
                {
                    await LoginProvider.TryLoginAsync(ip, port);
                }
            });
        }

        private void Log(string message)
        {
            P2PTracer.Info(message, "寻找设备");
        }

        private ConcurrentDictionary<string, Node> KnownNodeList { get; } = new ConcurrentDictionary<string, Node>();


        internal LocalIpProvider LocalIpProvider { get; }=new LocalIpProvider();

       

        //private async Task PingIp(string ip, string port)
        //{
        //    //Log($"开始扫描{ip}");
        //    if (KnownNodeList.ContainsKey(ip))
        //    {
        //        Log($"因为{ip}属于已知ip所以跳过");
        //        return;
        //    }

        //    //todo 修改端口
        //    var url = $"http://{ip}:{port}/";
        //    var beatUrl = $"{url}api/Peer/Beat";
        //    //todo 提升 HttpClient 性能
        //    var httpClient = new HttpClient();
        //    httpClient.Timeout = TimeSpan.FromSeconds(3);
        //    //Log($"开始访问{url}");
        //    try
        //    {
        //        var stringContent = new StringContent(CurrentDeviceInfoJson, Encoding.UTF8, "application/json");
        //        var message = await httpClient.PostAsync(beatUrl, stringContent);
        //        if (message.StatusCode == HttpStatusCode.OK)
        //        {
        //            var str = await message.Content.ReadAsStringAsync();
        //            var deviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(str);

        //            Log($"++ 连接{url}成功，勾搭了 {deviceInfo.DeviceName}");
        //            var node = new Node(url)
        //            {
        //                DeviceInfo = deviceInfo,
        //                LastUpdate = DateTime.Now
        //            };
        //            KnownNodeList.TryAdd(ip, node);

        //            //GetFriend(node);
        //        }
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        if
        //        (
        //            e.InnerException is SocketException socketException &&
        //            (
        //                socketException.ErrorCode == 10061 // 由于对方设备积极拒绝
        //                || socketException.ErrorCode == 10049 // 在其上下文中，该请求的地址无效
        //            )
        //        )
        //        {
        //            // 由于对方设备积极拒绝
        //            return;
        //        }

        //        Log($"连接{url}失败");
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        // 超时，对方设备不存在
        //    }
        //    catch (Exception e)
        //    {
        //        Log(e.ToString());
        //    }
        //}

        //private async void GetFriend(Node node)
        //{
        //    var url = $"{node.Url}api/Peer/GetFriend";
        //    var httpClient = new HttpClient();

        //    try
        //    {
        //        var nodeList = await httpClient.GetObjectAsync<List<Node>>(url);

        //        Log($"从{node}获取{nodeList.Count}设备");

        //        foreach (var temp in nodeList)
        //        {
        //            var (ip, port) = temp.GetIpFromNode();
        //            if (!KnownNodeList.ContainsKey(ip))
        //            {
        //                Log($"从{node}访问{temp}设备");
        //                await PingIp(ip, port);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        // HttpRequestException
        //        Log(e.ToString());
        //    }
        //}

        internal void AddOrUpdateKnownNode(string ip, IReadOnlyDeviceInfo deviceInfo)
        {
            var node = new Node($"http://{ip}:{deviceInfo.DevicePort}/")
            {
                DeviceInfo = deviceInfo,
                LastUpdate = DateTime.Now
            };

            AddOrUpdateKnownNode(ip, node);
        }

        internal bool TryUpdateKnownNode(string ip)
        {
            if (KnownNodeList.TryGetValue(ip, out var temp))
            {
                temp.LastUpdate = DateTime.Now;
                return true;
            }

            return false;
        }

        internal void AddOrUpdateKnownNode(string ip, Node node)
        {
            if (LocalIpProvider.IsLocal(ip))
            {
                return;
            }

            if (KnownNodeList.TryGetValue(ip, out var temp))
            {
                KnownNodeList.TryUpdate(ip, node, temp);
            }
            else
            {
                KnownNodeList.TryAdd(ip, node);
            }
        }

        public List<Node> SyncFriend(List<Node> nodeList)
        {
            // 对比不相同的
            var currentNodeList =
                GetCurrentNode()
                    .Where(temp => DateTime.Now - temp.LastUpdate < TimeSpan.FromMinutes(20)).ToHashSet();

            var newNode = new List<Node>();

            foreach (var node in nodeList)
            {
                if (currentNodeList.Contains(node))
                {
                    currentNodeList.Remove(node);
                }
                else
                {
                    newNode.Add(node);
                }
            }

            LoginProvider.RegisterLoginTask(newNode);

            return currentNodeList.ToList();
        }

      
    }
}