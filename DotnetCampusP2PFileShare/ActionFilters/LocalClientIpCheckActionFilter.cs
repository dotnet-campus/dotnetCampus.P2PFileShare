using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotnetCampusP2PFileShare.ActionFilters
{
    public class LocalClientIpCheckActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (CheckIsLocal(context.HttpContext))
            {
                base.OnActionExecuting(context);
            }
            else
            {
                context.Result = new NotFoundResult();
            }
        }

        private static bool CheckIsLocal(HttpContext httpContext)
        {
            var connection = httpContext.Connection;
            var ipAddress = connection.RemoteIpAddress;
            var localIpAddress = connection.LocalIpAddress;
            if (ipAddress is not null)
            {
                if (localIpAddress != null)
                {
                    return ipAddress.Equals(localIpAddress);
                }
                else
                {
                    return IPAddress.IsLoopback(ipAddress);
                }
            }
            else
            {
                if (localIpAddress == null)
                {
                    // 铁定是 TestServer 或者内转发的
                    return true;
                }
            }

            return false;
        }
    }
}
