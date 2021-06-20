using Microsoft.AspNetCore.Mvc;
using DotnetCampusP2PFileShare.Core.Peer;

namespace DotnetCampusP2PFileShare.Controllers
{
    /// <summary>
    /// 设备相关的包括设置或修改设备名，获取当前已经连接上的设备等
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        /// <inheritdoc />
        public DeviceController(PeerFinder peerFinder)
        {
            _peerFinder = peerFinder;
        }

        [HttpGet]
        [Route("DeviceInfo")]
        public IReadOnlyDeviceInfo GetDeviceInfo()
        {
            return AppConfiguration.Current.CurrentDeviceInfo;
        }

        [HttpGet]
        [Route(nameof(SetDeviceName))]
        public IActionResult SetDeviceName(string name)
        {
            AppConfiguration.Current.SetDeviceName(name);
            return Ok(AppConfiguration.Current.CurrentDeviceInfo);
        }

        /// <summary>
        /// 获取现在已经连接的设备数量
        /// </summary>
        [HttpGet]
        [Route("ConnectDeviceCount")]
        public int GetConnectDeviceCount()
        {
            return _peerFinder.GetCurrentNode().Count;
        }

        /// <summary>
        /// 获取现在已经连接的设备
        /// </summary>
        [HttpGet]
        [Route("ConnectDevice")]
        public IActionResult GetConnectDevice()
        {
            return Ok(_peerFinder.GetCurrentNode());
        }

        private readonly PeerFinder _peerFinder;
    }
}