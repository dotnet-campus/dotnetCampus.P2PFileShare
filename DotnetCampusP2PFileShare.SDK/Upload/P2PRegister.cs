using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotnetCampusP2PFileShare.Model;

namespace DotnetCampusP2PFileShare.SDK.Upload
{
    public class P2PRegister
    {
        public P2PRegister(P2PProvider p2PProvider)
        {
            P2PProvider = p2PProvider;
        }

        public P2PProvider P2PProvider { get; }

        public async Task<bool> RegisterResourceAsync(UploadResourceInfo uploadResourceInfo)
        {
            return await RegisterResourceAsync(uploadResourceInfo.ResourceId,
                new FileInfo(uploadResourceInfo.LocalPath));
        }

        public async Task<bool> RegisterResourceAsync(string resourceId, FileInfo file)
        {
            if (!P2PProvider.P2PProcess.TryStart())
            {
                return false;
            }

            file.Refresh();
            if (!file.Exists || file.Length < 1024 * 100)
            {
                // 文件存在的，同时长度大于100K的才能注册
                return false;
            }

            var url = $"{P2PProvider.P2PHost}api/Resource/Upload";

            var uploadResourceInfo = new UploadResourceInfo
            {
                LocalPath = file.FullName,
                ResourceId = resourceId
            };

            try
            {
                var message = await PostAsync(url, uploadResourceInfo);
                if (message?.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
            }

            return false;
        }

        private Task<HttpWebResponse> PostAsync(string url, UploadResourceInfo uploadResourceInfo)
        {
            //var httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(uploadResourceInfo);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");
            //try
            //{

            //    //httpClient.PostAsync(url, content);

            //    //return message;
            //}
            //catch (Exception e)
            //{
            //    return null;
            //}

            // 为什么需要用这么不清真方法，因为原来方法会让进程退出
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            return Task.FromResult(httpResponse);
        }
    }
}