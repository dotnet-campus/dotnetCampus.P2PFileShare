using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotnetCampusP2PFileShare.Core.Peer;

namespace DotnetCampusP2PFileShare.Core.Net
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetObjectAsync<T>(this HttpClient httpClient, string url)
        {
            var str = await httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static async Task<T> ReadAsAsync<T>(this HttpContent httpContent)
        {
            var json = await httpContent.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static (string ip, string port) GetIpFromNode(this Node node)
        {
            return GetIpFromUrl(node.Url);
        }

        private static (string ip, string port) GetIpFromUrl(string url)
        {
            var regex = IpRegex;

            var match = regex.Match(url);

            var ip = $"{match.Groups[1]}.{match.Groups[2]}.{match.Groups[3]}.{match.Groups[4]}";
            var port = match.Groups[5].Value;
            return (ip, port);
        }

        private static readonly Regex IpRegex = new Regex(@"http[s]://(\d+)\.(\d+)\.(\d+)\.(\d+):(\d+)/",
            RegexOptions.Compiled);
    }
}