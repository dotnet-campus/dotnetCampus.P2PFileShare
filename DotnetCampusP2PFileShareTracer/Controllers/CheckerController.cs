using Microsoft.AspNetCore.Mvc;

namespace DotnetCampusP2PFileShareTracer.Controllers
{
    /// <summary>
    /// 用于给鲸云访问
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}