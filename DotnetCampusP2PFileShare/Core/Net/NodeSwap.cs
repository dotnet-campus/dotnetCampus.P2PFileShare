using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotnetCampusP2PFileShare.Core.Peer;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Net
{
    /// <summary>
    /// 设备信息交换
    /// </summary>
    /// 这不是一个类，这个功能是一个程序集
    public class NodeSwap
    {
        /// <summary>
        /// 向指定的设备发送信息
        /// </summary>
        /// <param name="node"></param>
        /// <param name="relativeUri"></param>
        /// <param name="data"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static async Task<(bool success, HttpResponseMessage httpResponseMessage)> SendMessage(Node node,
            string relativeUri, object data,
            HttpClient httpClient = null)
        {
            return await SendMessage(node, relativeUri, JsonSerializer.Serialize(data), httpClient);
        }

        public static async Task<(bool success, T respond)> SendMessageAndGetRespondAsync<T>(Node node,
            string relativeUri, object data,
            HttpClient httpClient = null)
        {
            var (success, message) = await SendMessage(node, relativeUri, JsonSerializer.Serialize(data), httpClient);

            if (!success)
            {
                return (false, default);
            }

            try
            {
                if (message.StatusCode == HttpStatusCode.OK)
                {
                    var json = await message.Content.ReadAsStringAsync();

                    // 有内容的欢迎，加上时间权限，下一次优先访问
                    node.LastUpdate = DateTime.Now.AddSeconds(10);

                    return (true, JsonSerializer.Deserialize<T>(json));
                }

                P2PTracer.Debug($"访问 {node} 返回 {message.StatusCode}");
            }
            catch (Exception e) when (e is TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return (false, default);
        }

        /// <summary>
        /// 向指定的设备发送信息
        /// </summary>
        /// <param name="node"></param>
        /// <param name="relativeUri"></param>
        /// <param name="json"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static async Task<(bool success, HttpResponseMessage httpResponseMessage)> SendMessage(Node node,
            string relativeUri, string json,
            HttpClient httpClient = null)
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            var nodeUrl = new Uri(node.Url);
            var url = new Uri(nodeUrl, relativeUri);

            try
            {
                var httpResponseMessage =
                    await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                node.LastUpdate = DateTime.Now;
                return (true, httpResponseMessage);
            }
            catch (Exception e) when (e is TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return (false, null);
        }
    }
}