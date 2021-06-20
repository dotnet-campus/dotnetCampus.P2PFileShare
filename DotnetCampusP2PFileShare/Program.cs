using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using DotnetCampusP2PFileShare.Core;
using DotnetCampusP2PFileShare.Core.Context;
using DotnetCampusP2PFileShare.Data;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare
{
    public class Program
    {
        private static Mutex _mutex;

        public static void Main(string[] args)
        {
            using var mutex = new Mutex(true, Const.LockId, out var created);

            _mutex = mutex;

            if (!created)
            {
                Console.WriteLine("已经有 P2P 服务启动");
                Task.Delay(TimeSpan.FromSeconds(1));
#if !DEBUG
                return;
#endif
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            FileManagerContext.Migrate();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var port = FindPort();
                    webBuilder.UseUrls($"http://0.0.0.0:{port}");
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            var message = new StringBuilder();
            foreach (var exception in e.Exception.InnerExceptions)
            {
                message.Append($"{exception.Message} \r\n {exception.StackTrace} \r\n\r\n");
            }

            P2PTracer.Report(message.ToString(), EventId.DotnetCampusP2PFileShareUnhandledException);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var description = e.ExceptionObject.ToString();
            var message = description;
            if (e.ExceptionObject is Exception exception)
            {
                message += $"\r\n {exception.StackTrace} \r\n {exception.Source}";
            }

            P2PTracer.Report(message, EventId.DotnetCampusP2PFileShareUnhandledException, description: description);

            if (e.IsTerminating)
            {
                _mutex.Dispose();

                Rerun();
            }
        }

        /// <summary>
        /// 尝试重启
        /// </summary>
        private static void Rerun()
        {
            var hostApplicationLifetime = Container.Get<IHostApplicationLifetime>();
            hostApplicationLifetime.StopApplication();

            //todo 自动重启
        }

        private static int FindPort()
        {
            var port = PortPicker.GetNextAvailablePort();
            AppConfiguration.Current.CurrentDeviceInfo.DevicePort = port.ToString();

            return port;
        }
    }
}