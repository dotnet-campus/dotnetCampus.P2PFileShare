using System.Text.RegularExpressions;

namespace DotnetCampusP2PFileShare.Core.Peer.Finder
{
    class IpRegex
    {
        public static (string ip, string port) Parse(string str)
        {
            var regex = new Regex(@"(\d+\.\d+\.\d+\.\d+):(\d+)");

            var match = regex.Match(str);

            if (match.Success)
            {
                var ip = match.Groups[1].Value;
                var port = match.Groups[2].Value;

                return (ip, port);
            }

            return default;
        }
    }
}