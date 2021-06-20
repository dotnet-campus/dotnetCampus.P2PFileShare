using System;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.Web.Tests.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Extensions.Contracts;

namespace DotnetCampusP2PFileShare.Web.Tests.Controllers
{
    [TestClass()]
    public class ResourceControllerTests
    {
        [ContractTestCase]
        public void DownloadTest()
        {
            "执行下载动作，可以返回成功执行下载动作".Test(async () =>
            {
                var testClient = TestHostBuild.GetTestClient();

                var downloadFileInfo = new DownloadFileInfo()
                {
                    DownloadFolderPath = Path.GetTempPath(),
                    FileId = Guid.NewGuid().ToString(),
                    FileName = Path.GetRandomFileName(),
                };

                var result = await testClient.PostAsJsonAsync("api/Resource/Download", downloadFileInfo);

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            });
        }
    }
}