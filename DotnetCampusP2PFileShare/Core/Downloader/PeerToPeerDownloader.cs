using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DotnetCampusP2PFileShare.Core.FileStorage;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Downloader
{
    public class PeerToPeerDownloader
    {
        /// <inheritdoc />
        public PeerToPeerDownloader(ResourceSniffer resourceSniffer, IHttpClientFactory httpClientFactory,
            ILogger<PeerToPeerDownloader> logger, ProcessToken processReport, FileManager fileManager)
        {
            HttpClientFactory = httpClientFactory;
            _resourceSniffer = resourceSniffer;
            _logger = logger;
            _processReport = processReport;
            _fileManager = fileManager;
        }

        public IHttpClientFactory HttpClientFactory { get; }

        public async Task Download(DownloadFileInfo downloadFileInfo)
        {
            DownloadTracer.StartSniff();

            var resourceSniffer = _resourceSniffer;
            var inspectionResource = new InspectionResource
            {
                ResourceId = downloadFileInfo.FileId
            };

            await foreach (var folderResource in resourceSniffer.Sniff(inspectionResource))
            {
                DownloadTracer.FirstSniffResource();

                // 开始下载
                if (folderResource.FileResourceList.Count == 1)
                {
                    await SingleFileDownloadAsync(folderResource.FileResourceList[0], downloadFileInfo);
                }

                // 暂时只从第一个设备下载
                break;
            }
        }

        private readonly FileManager _fileManager;
        private readonly ILogger<PeerToPeerDownloader> _logger;
        private readonly ProcessToken _processReport;
        private readonly ResourceSniffer _resourceSniffer;
        private P2PResourceDownloadTracer DownloadTracer => _processReport.DownloadTracer;

        /// <summary>
        /// 单文件下载
        /// </summary>
        /// <param name="fileResource"></param>
        /// <param name="downloadFileInfo"></param>
        private async Task SingleFileDownloadAsync(FileResource fileResource, DownloadFileInfo downloadFileInfo)
        {
            // 先判断文件夹是否存在
            var downloadFolder = downloadFileInfo.DownloadFolderPath;

            // 这个方法将会在文件夹不存在的时候创建文件夹，在文件夹存在的时候忽略
            Directory.CreateDirectory(downloadFolder);

            var fileName = downloadFileInfo.FileName;

            // 如果用户没有指定下载的文件名，那么就使用资源的文件名
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = fileResource.FileName;
            }

            DownloadTracer.Info($"开始从 {fileResource.DownloadUrl} 下载 {fileName} 到 {downloadFolder}");

            var downloadFile = new FileInfo(Path.Combine(downloadFolder, fileName + ".p2ptmp"));

            // 如果文件已经存在了，忽略
            if (downloadFile.Exists)
            {
                downloadFile.Delete();
            }

            using var webClient = new WebClient();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            await webClient.DownloadFileTaskAsync(fileResource.DownloadUrl, downloadFile.FullName);

            DownloadTracer.Info($"下载 {downloadFile.FullName} 完成");

            var file = new FileInfo(Path.Combine(downloadFolder, fileName));
            DownloadTracer.Info($"将 {downloadFile.FullName} 移动到 {file.FullName}");

            if (!file.Exists)
            {
                File.Move(downloadFile.FullName, file.FullName);

                file.Refresh();
                if (File.Exists(file.FullName))
                {
                    _processReport.SetFinished(file);

                    // 注册资源
                    _fileManager.AddResource(new UploadResourceInfo
                    {
                        LocalPath = file.FullName,
                        ResourceId = downloadFileInfo.FileId
                    });
                }
                else
                {
                    _processReport.SetFail($"文件 {file.FullName} 被删除");
                }
            }
            else
            {
                _processReport.SetFail($"因为 {file.FullName} 存在，移动失败");
            }

            //var httpClient = HttpClientFactory.CreateClient();

            //var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, fileResource.DownloadUrl);

            //var message = await httpClient.SendAsync(httpRequestMessage);

            //var fileStream = downloadFile.Create();
            //using var content = message.Content;
            //var stream = await content.ReadAsStreamAsync();
            //stream.CopyTo(fileStream);

            //stream.Dispose();
            //fileStream.Dispose();
            //message.Dispose();
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _processReport.SetProcess(e.ProgressPercentage);
        }
    }

    /// <summary>
    /// 链文件下载类
    /// 链文件下载的详细请看P2P文档
    /// </summary>
    public class LinkFileDownloader
    {
        /// <inheritdoc />
        public LinkFileDownloader(string resourceId, DownloadFileInfo downloadFileInfo, Uri originUrl)
        {
            ResourceId = resourceId;
            DownloadFileInfo = downloadFileInfo;
            OriginUrl = originUrl;
        }

        public string ResourceId { get; }

        public DownloadFileInfo DownloadFileInfo { get; }

        public Uri OriginUrl { get; }

        public void DownloadFile()
        {
            //todo 评估设备下载性能计算
            //todo 多个设备下载方法
        }

        /// <summary>
        /// 在找到其他设备的过程将会不断加入
        /// </summary>
        public void AddDownloadNode()
        {
        }
    }
}