using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetCampusP2PFileShare.Web.Tests.Framework
{
    [TestClass]
    public static class TestHostBuild
    {
        public static HttpClient GetTestClient() => _host.GetTestClient();

        [AssemblyInitialize]
        public static async Task GlobalInitialize(TestContext testContext)
        {
            IHost host = await CreateAndRun();
            _host = host;
        }

        private static IHost _host;

        [AssemblyCleanup]
        public static void GlobalCleanup()
        {
            _host.Dispose();
        }

        private static Task<IHost> CreateAndRun() => CreateHostBuilder().StartAsync();

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseTestServer(); //关键是多了这一行建立TestServer
                });
    }
}
