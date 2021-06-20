using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.Core.Peer
{
    /// <summary>
    /// 管理本地已知设备
    /// </summary>
    internal class LocalFileKnownDeviceManager
    {
        /// <inheritdoc />
        public LocalFileKnownDeviceManager(ConcurrentDictionary<string, Node> knownNodeList)
        {
            KnownNodeList = knownNodeList;
            KnownDeviceFile = Path.Combine(AppConfiguration.Current.ConfigurationFolder,
                $"{nameof(KnownDeviceFile)}.txt");
        }

        /// <summary>
        /// 读取上一次找到的设备
        /// </summary>
        public async Task<List<(string ip, string port)>> ReadFromFileAsync()
        {
            var knownDeviceFile = KnownDeviceFile;
            var knownList = new List<(string ip, string port)>();
            if (File.Exists(knownDeviceFile))
            {
                var regex = new Regex(@"http://(\d+)\.(\d+)\.(\d+)\.(\d+):(\d+)/",
                    RegexOptions.Compiled);

                foreach (var temp in await File.ReadAllLinesAsync(knownDeviceFile))
                {
                    var match = regex.Match(temp);

                    var ip = $"{match.Groups[1]}.{match.Groups[2]}.{match.Groups[3]}.{match.Groups[4]}";
                    var port = match.Groups[5].Value;
                    knownList.Add((ip, port));
                }
            }
            else
            {
                Log("没有找到已知设备文件");
            }

            return knownList;
        }

        /// <summary>
        /// 自动后台写入已知设备
        /// </summary>
        public void AutoTaskToWriteKnownList()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    if (KnownNodeList.Count == 0)
                    {
                        continue;
                    }

                    var knownUrl = string.Join("\r\n",
                        KnownNodeList.ToList().Select(temp => temp.Value.Url));

                    try
                    {
                        await File.WriteAllTextAsync(KnownDeviceFile, knownUrl);
                    }
                    catch (Exception e)
                    {
                        Log(e.ToString());
                    }
                }
            });
        }

        private string KnownDeviceFile { get; }

        private void Log(string message)
        {
        }

        private ConcurrentDictionary<string, Node> KnownNodeList { get; }
    }
}