using System.Net;
using DotnetCampusP2PFileShare.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DotnetCampusP2PFileShare.Core.Downloader;
using DotnetCampusP2PFileShare.Core.FileStorage;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Controllers
{
    /// <summary>
    /// 用于和本机通信
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ResourceController : ControllerBase
    {
        /// <inheritdoc />
        public ResourceController(ResourceDownloader resourceDownloader, FileManager fileManager,
            ILogger<ResourceController> logger, ProcessToken processReport, ProcessManager processManager)
        {
            _resourceDownloader = resourceDownloader;
            _fileManager = fileManager;
            _logger = logger;
            _processReport = processReport;
            _processManager = processManager;
        }

        [HttpPost]
        [Route("Download")]
        [ServiceFilter(typeof(LocalClientIpCheckActionFilter))]// 只能在本机内访问
        public IActionResult Download([FromBody] DownloadFileInfo downloadFileInfo)
        {
            var p2PResourceDownloadTracer = P2PTracer.GetResourceDownloadTracer(downloadFileInfo.FileId);
            _processReport.DownloadTracer = p2PResourceDownloadTracer;

            p2PResourceDownloadTracer.DownloadRequest();

            _processManager.Register(_processReport);

            _resourceDownloader.Download(downloadFileInfo);

            return Ok(_processReport.ProcessReporter);
        }

        [HttpGet]
        [Route("DownloadProcess")]
        // 获取下载进度？不是本机也允许获取 [ServiceFilter(typeof(LocalClientIpCheckActionFilter))]
        public IActionResult GetDownloadProcess(string id)
        {
            if (_processManager.TryGetProcessReport(id, out var processReport))
            {
                return Ok(processReport);
            }

            return NotFound();
        }

        /// <summary>
        /// 将本机的资源上传到 P2P 网络上
        /// </summary>
        /// <remarks>
        /// 本质上只是在当前的 P2P 服务注册就可以了，不需要真的上传文件到某个服务器上
        /// </remarks>
        /// <param name="uploadResourceInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Upload")]
        [ServiceFilter(typeof(LocalClientIpCheckActionFilter))]// 只能在本机内访问，将本机的资源上传到 P2P 网络上
        public IActionResult Upload([FromBody] UploadResourceInfo uploadResourceInfo)
        {
            // 判断用户上传的文件还是文件夹
            P2PTracer.Info($"用户上传资源{uploadResourceInfo.ResourceId}");

            _fileManager.AddResource(uploadResourceInfo);

            return Ok();
        }

        private readonly FileManager _fileManager;
        private readonly ILogger<ResourceController> _logger;
        private readonly ProcessManager _processManager;
        private readonly ProcessToken _processReport;
        private readonly ResourceDownloader _resourceDownloader;
    }
}