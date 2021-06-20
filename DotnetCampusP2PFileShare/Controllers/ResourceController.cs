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
        public IActionResult GetDownloadProcess(string id)
        {
            if (_processManager.TryGetProcessReport(id, out var processReport))
            {
                return Ok(processReport);
            }

            return NotFound();
        }

        [HttpPost]
        [Route("Upload")]
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