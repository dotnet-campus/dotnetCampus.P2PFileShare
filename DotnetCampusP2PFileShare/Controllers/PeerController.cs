using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DotnetCampusP2PFileShare.Core.Context;
using DotnetCampusP2PFileShare.Core.FileStorage;
using DotnetCampusP2PFileShare.Core.Peer;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeerController : ControllerBase
    {
        /// <inheritdoc />
        public PeerController(FileManager fileManager, PeerFinder peerFinder, AppConfiguration appConfiguration)
        {
            _fileManager = fileManager;
            _peerFinder = peerFinder;
            _appConfiguration = appConfiguration;
        }

        /// <summary>
        /// 测试使用的方法
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(Beat))]
        public IActionResult Beat()
        {
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            P2PTracer.Debug($"收到 {ip} 访问");

            return Ok(_appConfiguration.CurrentDeviceInfo);
        }

        [HttpPost]
        [Route("Beat")]
        public IActionResult PostBeat([FromBody] DeviceInfo deviceInfo)
        {
            //todo 如果对方版本代替，推送他升级
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();

            P2PTracer.Info($"收到 {deviceInfo?.DeviceName} {ip} 访问", "PeerBeat");

            _peerFinder.AddOrUpdateKnownNode(ip, deviceInfo);

            //todo 同时返回客户端的主IP地址，这样对方就可以用这个IP作为自己的主地址
            return Ok(_appConfiguration.CurrentDeviceInfo);
        }

        /// <summary>
        /// 让其他设备可以注册
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(Login))]
        public IActionResult Login([FromBody] DeviceInfo deviceInfo)
        {
            // 和关系维护不相同的是，将会返回更多信息，例如对方的主 IP 是哪个
            // 同时返回已经连接的设备
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();

            P2PTracer.Info($"收到 {deviceInfo?.DeviceName} {ip} 访问", "PeerLogin");

            _peerFinder.AddOrUpdateKnownNode(ip, deviceInfo);

            const int maxFriendCount = 100;

            var loginInfo = new
            {
                MainIp = ip,
                DeviceInfo = _appConfiguration.CurrentDeviceInfo,
                Friends = _peerFinder.GetCurrentNode().Take(maxFriendCount).ToList()
            };
            return Ok(loginInfo);
        }

        /// <summary>
        /// 同步当前能连接的伙伴
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(SyncFriend))]
        public IActionResult SyncFriend(List<Node> nodeList)
        {
            return Ok(_peerFinder.SyncFriend(nodeList));
        }

        [HttpGet]
        [Route("GetFriend")]
        public IActionResult GetFriend()
        {
            return Ok(_peerFinder.GetCurrentNode());
        }

        [HttpPost]
        [Route(nameof(FindResource))]
        public IActionResult FindResource([FromBody] InspectionResource id)
        {
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            if (!_peerFinder.TryUpdateKnownNode(ip))
            {
                _peerFinder.LoginProvider.RegisterLoginTask(ip, Const.DefaultPort.ToString());
            }

            var tracer = P2PTracer.GetP2PResourceUploadTracer(id.ResourceId);
            tracer.FindResourceRequest(ip);

            var fileManager = _fileManager;
            if (fileManager.TryFindResource(id.ResourceId, out var resource))
            {
                tracer.FindResourceSuccess();

                return Ok(resource);
            }

            tracer.CanNotFindResource();

            return NotFound("在本机没有找到资源" + id);
        }

        /// <summary>
        /// 提供单文件下载
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="relativePath">如果是文件夹，将会提供相对路径</param>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(DownloadFile))]
        public IActionResult DownloadFile(string resourceId, string relativePath)
        {
            if (_fileManager.TryGetFile(resourceId, relativePath, out var file))
            {
                var ip = HttpContext.Connection.RemoteIpAddress.ToString();

                P2PTracer.Report($"上传{resourceId} {relativePath}资源给{ip}设备，资源大小{file.Length}",
                    EventId.DotnetCampusP2PFileShareUploadResource, $"{resourceId} {relativePath}",
                    file.Length.ToString());

                const string mime = "application/octet-stream";

                return PhysicalFile(file.FullName, mime, false);
            }

            P2PTracer.GetP2PResourceUploadTracer(resourceId).CanNotFindUploadFile(relativePath);
            return NotFound($"传入的 {resourceId} 没有找到对应的文件");
        }

        private readonly AppConfiguration _appConfiguration;
        private readonly FileManager _fileManager;
        private readonly PeerFinder _peerFinder;
    }
}