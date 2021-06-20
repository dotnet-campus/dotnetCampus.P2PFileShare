using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.P2PLogging
{
    public class LogFileManager
    {
        static LogFileManager()
        {
            LogFolder = new DirectoryInfo(Path.Combine(AppConfiguration.Current.ConfigurationFolder, "Logs"));
        }

        public LogFileManager()
        {
            WriteToFile();
        }

        public static DirectoryInfo LogFolder { set; get; }

        public FileInfo LogFile { set; get; }

        public static void CleanLogFile()
        {
            if (LogFolder == null) return;
            try
            {
                var time = TimeSpan.FromDays(7);
                foreach (var temp in LogFolder.GetFiles())
                {
                    if (DateTime.Now - temp.CreationTime > time)
                    {
                        try
                        {
                            temp.Delete();
                        }
                        catch (Exception)
                        {
                            // 删除文件
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void WriteLine(string message)
        {
            var time = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.ffffff");
            _cache.Enqueue($"{time} {message}");

            _asyncAutoResetEvent.Set();
        }

        private readonly AsyncAutoResetEvent _asyncAutoResetEvent = new AsyncAutoResetEvent(false);

        private readonly ConcurrentQueue<string> _cache = new ConcurrentQueue<string>();

        private void WriteToFile()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await _asyncAutoResetEvent.WaitOneAsync();

                    var count = Math.Max(_cache.Count, 8);

                    var cache = new List<string>(count);

                    while (_cache.TryDequeue(out var message))
                    {
                        cache.Add(message);
                    }

                    if (LogFile is null)
                    {
                        var folder = LogFolder?.FullName ?? "";
                        if (!string.IsNullOrEmpty(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        var id = Process.GetCurrentProcess().Id;
                        var time = DateTime.Now.ToString("yyMMddhhmmss");
                        var file = Path.Combine(folder, $"{time} {id}.txt");

                        LogFile = new FileInfo(file);
                    }

                    await File.AppendAllLinesAsync(LogFile.FullName, cache);
                }
            });
        }
    }
}