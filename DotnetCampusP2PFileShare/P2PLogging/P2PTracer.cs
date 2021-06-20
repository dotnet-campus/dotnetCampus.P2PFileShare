using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lindexi.src;
using Newtonsoft.Json;
using DotnetCampusP2PFileShare.Core.Peer;
using TraceLevel = System.Diagnostics.TraceLevel;

namespace DotnetCampusP2PFileShare.P2PLogging
{
    interface ILogTracer: ITracer
    {

    }

    interface ITracer
    {

    }

    public class Reporter
    {
        public void ReportDiagnosis(string version, int i, string deviceInfoDeviceId, string macList, string sessionId, string businessType, string secondType, string thirdType, string forthType, string message, string description, int i1, int i2, object o)
        {
            
        }
    }

    public class P2PTracer : ILogTracer
    {
        private P2PTracer()
        {
            _traceLevel = TraceLevel.Verbose;

            var task = Task.Run(InitReporter);
            _reporter = new Lazy<Reporter>(() =>
            {
                task.Wait();
                var reporter = new Reporter();
                return reporter;
            });
        }

        public static LogFileManager LogFileManager { get; } = new LogFileManager();

        public static P2PResourceDownloadTracer GetResourceDownloadTracer(string resourceId)
        {
            return new P2PResourceDownloadTracer(resourceId);
        }

        public static P2PResourceUploadTracer GetP2PResourceUploadTracer(string resourceId)
        {
            if (P2PResourceUploadTracerList.TryGetValue(resourceId, out var value))
            {
                if (value.TryGetTarget(out var p2PResourceUploadTracer))
                {
                    return p2PResourceUploadTracer;
                }

                P2PResourceUploadTracerList.TryRemove(resourceId, out _);
            }

            var tracer = new P2PResourceUploadTracer(resourceId);

            P2PResourceUploadTracerList.TryAdd(resourceId, new WeakReference<P2PResourceUploadTracer>(tracer));

            return tracer;
        }

        public static void Info(string message, params string[] tag)
        {
            //Instance.Info(message, tag);
        }

        public static void Debug(string message, params string[] tag)
        {
            //Instance.Debug(message, tag);
        }

        public static void Report(string message, string type, string thirdType = "", string forthType = "",
            string description = "")
        {
            //Instance.Info(message, type);

            var deviceInfo = AppConfiguration.Current.CurrentDeviceInfo;
            var version = deviceInfo.Version;

            const string businessType = "DotnetCampusP2PFileShare";

            var p2PTracer = ((P2PTracer) Instance);
            var secondType = type;

            p2PTracer.Reporter.ReportDiagnosis(version, 0, deviceInfo.DeviceId, p2PTracer.MacList,
                p2PTracer.SessionId, businessType, secondType, thirdType, forthType, message, description, 0, 0,
                null);
        }

        public static void Report(Exception exception, string type)
        {
            var description = exception.ToString();
            var message = ExceptionToString(exception).ToString();

            Report(message, type, description: description);
        }

 
      

  

        private readonly Lazy<Reporter> _reporter;

        private TraceLevel _traceLevel;
        private IReadOnlyList<string> _tracerTags = new List<string>();

        /// <summary>
        /// 本机MAC地址
        /// </summary>
        private string MacList { set; get; }

        private string SessionId { set; get; }

        private void InitReporter()
        {
            MacList = string.Join(';', MacAddress.GetActiveMacAddress(":"));

            SessionId = Guid.NewGuid().ToString("N");
        }

        private Reporter Reporter => _reporter.Value;

        private static ConcurrentDictionary<string, WeakReference<P2PResourceUploadTracer>>
            P2PResourceUploadTracerList { get; } =
            new ConcurrentDictionary<string, WeakReference<P2PResourceUploadTracer>>();

        private static StringBuilder ExceptionToString(Exception exception)
        {
            var message = new StringBuilder($"{exception} \r\n {exception.StackTrace} \r\n {exception.Source}");

            if (exception.Data.Count > 0)
            {
                message.Append("\r\n");

                foreach (DictionaryEntry temp in exception.Data)
                {
                    message.Append($"{temp.Key} : {temp.Value} \r\n");
                }
            }

            if (exception is AggregateException aggregateException)
            {
                foreach (var temp in aggregateException.InnerExceptions)
                {
                    message.Append(ExceptionToString(temp));
                }
            }

            return message;
        }


        private static void Output(string message)
        {
            WriteToConsole(message);

            WriteToTrace(message);

            WriteToFile(message);
        }

        private static void WriteToTrace(string message)
        {
            Trace.TraceInformation(message);
        }

        private static void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        private static void WriteToFile(string message)
        {
            LogFileManager.WriteLine(message);
        }

        private static P2PTracer Instance { get; } = new P2PTracer();

        private string BuildTags(string[] tags)
        {
            return TagsToString(new CombineReadonlyList<string>(tags, _tracerTags));
        }

        private static string TagsToString(IReadOnlyList<string> tags)
        {
            if (tags.Count == 0)
            {
                return "";
            }

            var stringBuilder = new StringBuilder();
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];

                var last = i == tags.Count - 1;
                var separator = "";

                if ((tag.StartsWith('[') && tag.EndsWith(']')) ||
                    (tag.StartsWith('【') && tag.EndsWith('】')))
                {
                }
                else
                {
                    tag = $"[{tag}]";
                }

                stringBuilder.Append(tag);
                if (!last)
                {
                    stringBuilder.Append(separator);
                }
            }

            return stringBuilder.ToString();
        }
    }

    public class P2PRunningTracer
    {
        public P2PRunningTracer(PeerFinder peerFinder)
        {
            PeerFinder = peerFinder;

            StartTime = DateTime.Now;
        }

        public PeerFinder PeerFinder { get; }

        /// <summary>
        /// 启动
        /// </summary>
        public void ReportStart()
        {
            // 不需要统计启动时长
            P2PTracer.Report(PeerFinder.CurrentDeviceInfoJson, EventId.DotnetCampusP2PFileShareStartUp);
        }

        /// <summary>
        /// 上报设备信息
        /// </summary>
        public async void ReportDevice()
        {
            while (true)
            {
                var node = PeerFinder.GetCurrentNode();

                var message = $"找到{node.Count}设备，";
                const int maxCount = 10;
                if (node.Count < maxCount)
                {
                    message += "设备列表如下";
                }
                else
                {
                    message += $"其中前{maxCount}个设备如下";
                }

                var json = JsonConvert.SerializeObject(new
                {
                    CurrentNode = PeerFinder.CurrentDeviceInfo,
                    Message = message,
                    NodeList = node.Take(maxCount)
                });

                P2PTracer.Report(json, EventId.DotnetCampusP2PFileShareConnectNode, description: $"找到{node.Count}设备",
                    thirdType: node.Count.ToString());

                await Task.Delay(TimeSpan.FromHours(1)).ConfigureAwait(false);
            }
        }

        private DateTime StartTime { get; }
    }

    public class P2PResourceUploadTracer
    {
        /// <inheritdoc />
        public P2PResourceUploadTracer(string resourceId)
        {
            ResourceId = resourceId;
        }

        public string ResourceId { get; }

        public string Ip { set; get; }

        /// <summary>
        /// 收到获取资源请求
        /// </summary>
        /// <param name="ip"></param>
        public void FindResourceRequest(string ip)
        {
            Ip = ip;
            P2PTracer.Info($"{ip} 在本机寻找资源 {ResourceId}");
        }

        /// <summary>
        /// 成功找到资源
        /// </summary>
        public void FindResourceSuccess()
        {
            Info($"在本机可以找到{ResourceId}资源");
        }

        public void CanNotFindResource()
        {
            Info($"在本机没有找到{ResourceId}资源");
        }

        public void CanNotFindUploadFile(string relativePath)
        {
            Info($"传入的 {ResourceId} {relativePath}没有找到对应的文件");
        }

        private void Info(string message)
        {
            P2PTracer.Info(message, "资源上传");
        }
    }

    internal class TraceStopwatch
    {
        public void Start(string key)
        {
            if (!StopwatchList.ContainsKey(key))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                StopwatchList.Add(key, stopwatch);
            }
        }

        public long End(string key)
        {
            if (StopwatchList.TryGetValue(key, out var stopwatch))
            {
                stopwatch.Stop();
                StopwatchList.Remove(key);

                return stopwatch.ElapsedMilliseconds;
            }

            return -1;
        }

        private Dictionary<string, Stopwatch> StopwatchList { get; } = new Dictionary<string, Stopwatch>();
    }
}