using System.Collections.Generic;

namespace DotnetCampusP2PFileShareTracer
{
    public class Device
    {
        public string MainIp { set; get; }

        public int DeviceCount { set; get; }

        public List<Node> NodeList { set; get; }
    }
}