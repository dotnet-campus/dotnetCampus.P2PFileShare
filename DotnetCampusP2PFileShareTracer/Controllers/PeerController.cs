﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using DotnetCampusP2PFileShareTracer.Data;

namespace DotnetCampusP2PFileShareTracer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeerController : ControllerBase
    {
        public PeerController(NodeContext context, ILogger<PeerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("Device")]
        public IActionResult GetDevice()
        {
            var ipList = new Dictionary<string, List<Node>>();

            foreach (var node in _context.Node)
            {
                if (ipList.ContainsKey(node.MainIp))
                {
                    ipList[node.MainIp].Add(node);
                }
                else
                {
                    ipList[node.MainIp] = new List<Node>() { node };
                }
            }

            var deviceList = new List<Device>();
            foreach (var nodeList in ipList)
            {
                var device = new Device()
                {
                    MainIp = nodeList.Key,
                    NodeList = nodeList.Value
                };
                device.DeviceCount = device.NodeList.Count;

                deviceList.Add(device);
            }

            return Ok(deviceList);
        }

        [HttpGet("{localIp}")]
        public IActionResult GetPeer(string localIp)
        {
            var ip = GetIp();

            var nodeList = _context.Node.Where(temp => temp.MainIp == ip).ToList();

            var removeList = new List<Node>();

            for (var i = 0; i < nodeList.Count; i++)
            {
                if (DateTime.Now - nodeList[i].LastUpdate > TimeSpan.FromHours(2))
                {
                    removeList.Add(nodeList[i]);
                    nodeList.RemoveAt(i);
                    i--;
                }
            }

            var node = nodeList.FirstOrDefault(temp => temp.LocalIp == localIp);

            _logger.LogInformation($"ip={ip} localip {localIp} count = {nodeList.Count}");

            if (node != null)
            {
                _context.Node.Remove(node);
                nodeList.Remove(node);
            }

            _context.Node.Add(new Node
            {
                MainIp = ip,
                LocalIp = localIp,
                LastUpdate = DateTime.Now
            });
            _context.Node.RemoveRange(removeList);

            _context.SaveChanges();
            return Ok(string.Join(';', nodeList.Select(temp => temp.LocalIp)));
        }

        private readonly NodeContext _context;
        private readonly ILogger<PeerController> _logger;

        private string GetIp()
        {
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();

            if (TryGetUserIpFromFrp(HttpContext.Request, out var frp))
            {
                ip = frp;
            }

            return ip;
        }

        private static bool TryGetUserIpFromFrp(HttpRequest httpContextRequest, out StringValues ip)
        {
            return httpContextRequest.Headers.TryGetValue("X-Forwarded-For", out ip);
        }
    }
}