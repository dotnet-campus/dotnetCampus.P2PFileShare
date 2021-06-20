using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotnetCampusP2PFileShare.Core;
using DotnetCampusP2PFileShare.Core.Application;
using DotnetCampusP2PFileShare.Core.Application.Updater;
using DotnetCampusP2PFileShare.Core.Downloader;
using DotnetCampusP2PFileShare.Core.FileStorage;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.Core.Peer;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appConfiguration = AppConfiguration.Current;

            services.AddControllers()
                .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

            services.AddHttpClient();

            var peerFinder = new PeerFinder();
            Task.Run(peerFinder.FindPeer);

            services.AddSingleton(appConfiguration);
            services.AddSingleton(peerFinder);
            services.AddSingleton(new FileManager());

            services.AddSingleton<ProcessManager>();

            services.AddScoped<ProcessToken>();
            services.AddScoped<ResourceSniffer>();
            services.AddScoped<ResourceDownloader>();
            services.AddScoped<PeerToPeerDownloader>();

            Container.RegisterServiceProvider(services.BuildServiceProvider());

            var p2PRunningTracer = new P2PRunningTracer(peerFinder);
            DelayTask.AddTask(p2PRunningTracer.ReportStart);
            DelayTask.AddTask(LogFileManager.CleanLogFile, TimeSpan.FromMinutes(10));
            DelayTask.AddTask(p2PRunningTracer.ReportDevice,
                // 大概扫描网段的时间在30分钟都能扫完，也就是 255 个IP假设一分钟10个IP那么也就是25分钟扫完
                TimeSpan.FromMinutes(30));
            // 自动更新
            DelayTask.AddTask(new AutoUpdate().Update, TimeSpan.FromMinutes(10));
            //RunTimeTest.AddTest(() => { DelayTask.AddTask(p2PRunningTracer.ReportDevice); });

            DelayTask.Run();
            //services.AddDbContext<FileManagerContext>(options =>
            //        options.UseSqlite("Filename=./FileManger.db"));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime applicationLifetime)
        {
            Container.Set(applicationLifetime);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}