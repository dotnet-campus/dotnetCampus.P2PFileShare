using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DotnetCampusP2PFileShare.Controllers;
using DotnetCampusP2PFileShare.Core.FileStorage;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.Core.Peer;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core
{
    /// <summary>
    /// 寻找资源
    /// </summary>
    public class ResourceSniffer
    {
        /// <inheritdoc />
        public ResourceSniffer(PeerFinder peerFinder, ILogger<ResourceSniffer> logger, ProcessToken processReport)
        {
            _peerFinder = peerFinder;
            _logger = logger;
            _processReport = processReport;
        }

        public P2PResourceDownloadTracer DownloadTracer => _processReport.DownloadTracer;

        /// <summary>
        /// 寻找资源
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<FolderResource> Sniff(InspectionResource fileId)
        {
            var nodeList = _peerFinder.GetCurrentNode();

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            // 按照访问的时间排序
            // 每次询问最多10设备
            nodeList = nodeList.OrderByDescending(temp => temp.LastUpdate).ToList();
            var n = 0;
            var canFindResource = false;
            while (n < nodeList.Count)
            {
                const int maxCount = 10;
                var taskList = new List<Task<(bool success, FolderResource folderResource)>>(maxCount);
                for (var i = n; i < n + maxCount && i < nodeList.Count; i++)
                {
                    var node = nodeList[i];
                    DownloadTracer.Debug($"询问 {node} 是否存在资源，设备{i + 1}/{nodeList.Count}");
                    taskList.Add(TryFindResourceAsync(node, fileId, httpClient));
                }

                for (var i = 0; i < maxCount && taskList.Count > 0; i++)
                {
                    Task<(bool success, FolderResource folderResource)> task = null;
                    FolderResource resource = null;
                    try
                    {
                        task = await Task.WhenAny(taskList);
                        var (success, folderResource) = task.Result;
                        if (success)
                        {
                            resource = folderResource;
                            canFindResource = true;
                        }
                    }
                    catch (Exception)
                    {
                        // 不需要
                    }

                    if (task != null)
                    {
                        taskList.Remove(task);
                    }

                    RemoveAbortTask(taskList);

                    if (resource != null)
                    {
                        yield return resource;
                    }
                }

                n += maxCount;
            }

            //for (var i = 0; i < nodeList.Count; i++)
            //{
            //    var node = nodeList[i];
            //    _logger.LogInformation($"询问 {node} 是否存在资源，设备{i+1}/{nodeList.Count}");

            //    var (success, message ) = await NodeSwap.SendMessage(node, "api/Peer/Find", fileId, httpClient);

            //    if (success && message.StatusCode == HttpStatusCode.OK)
            //    {
            //        var json = await message.Content.ReadAsStringAsync();
            //        var folderResource = JsonSerializer.Deserialize<FolderResource>(json, new JsonSerializerOptions()
            //        {
            //            PropertyNameCaseInsensitive = true
            //        });

            //        foreach (var fileResource in folderResource.FileResourceList)
            //        {
            //            var nodeUrl = new Uri(node.Url);
            //            if (Uri.TryCreate(nodeUrl, fileResource.DownloadUrl, out var absoluteUrl))
            //            {
            //                fileResource.DownloadUrl = absoluteUrl.AbsoluteUri;
            //            }
            //        }

            //        _logger.LogInformation($"在 {node} 找到资源 {fileId.ResourceId}");

            //        _processReport.SetProcess(10,$"在 {node} 找到资源 {fileId.ResourceId} 开始下载");
            //        // 可以开始下载了
            //        yield return folderResource;
            //    }
            //}

            if (!canFindResource)
            {
                _processReport.SetFail($"没有从已知设备找到 {fileId.ResourceId} 资源");
            }
        }

        private readonly ILogger<ResourceSniffer> _logger;
        private readonly PeerFinder _peerFinder;
        private readonly ProcessToken _processReport;

        private void RemoveAbortTask(List<Task<(bool success, FolderResource folderResource)>> taskList)
        {
            taskList.RemoveAll(temp => temp.IsCanceled || temp.IsFaulted);
        }

        private async Task<(bool success, FolderResource folderResource)> TryFindResourceAsync(Node node,
            InspectionResource fileId, HttpClient httpClient)
        {
            var (success, respond) =
                await NodeSwap.SendMessageAndGetRespondAsync<FolderResource>(node,
                    $"api/Peer/{nameof(PeerController.FindResource)}", fileId, httpClient);
            if (success)
            {
                var folderResource = respond;

                var nodeUrl = new Uri(node.Url);
                foreach (var fileResource in folderResource.FileResourceList)
                {
                    if (Uri.TryCreate(nodeUrl, fileResource.DownloadUrl, out var absoluteUrl))
                    {
                        fileResource.DownloadUrl = absoluteUrl.AbsoluteUri;
                    }
                }

                return (true, folderResource);
            }

            return (false, default);
        }
    }
}