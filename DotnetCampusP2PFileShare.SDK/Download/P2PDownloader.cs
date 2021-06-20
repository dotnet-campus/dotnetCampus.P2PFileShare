using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.SDK.Context;
using DotnetCampusP2PFileShare.SDK.Utils;

namespace DotnetCampusP2PFileShare.SDK.Download
{
    /// <summary>
    /// 下载器
    /// </summary>
    public class P2PDownloader
    {
        /// <inheritdoc />
        internal P2PDownloader(P2PProvider p2PProvider)
        {
            P2PProvider = p2PProvider;
        }

        public P2PProvider P2PProvider { get; }

        /// <summary>
        /// 使用P2P下载资源
        /// </summary>
        /// <param name="downloadFileEntry"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<DownloadCompletedArgs> DownloadFileAsync(P2PDownloadFileEntry downloadFileEntry,
            IProgress<DownloadProgress> progress,
            CancellationTokenSource cancellationTokenSource = null)
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            return await DownloadFromP2P(downloadFileEntry.DownloadFile, downloadFileEntry, progress,
                cancellationTokenSource.Token);
        }

        /// <summary>
        /// 使用P2P下载资源，支持同时从局域网下载和原有下载
        /// </summary>
        /// <param name="downloadFileEntry">下载文件</param>
        /// <param name="progress">进度</param>
        /// <param name="straightDownloader">原有的下载方式，如果有设置值，那么将会同时从P2P和原有方式下载</param>
        /// <param name="cancellationTokenSource"></param>
        /// <param name="p2pDownloadTracer"></param>
        /// <returns></returns>
        public async Task<DownloadCompletedArgs> DownloadFileAsync(P2PDownloadFileEntry downloadFileEntry,
            IProgress<DownloadProgress> progress,
            IStraightDownloader straightDownloader,
            CancellationTokenSource cancellationTokenSource = null, P2PDownloadTracer p2pDownloadTracer = null)
        {
            p2pDownloadTracer = p2pDownloadTracer ?? new P2PDownloadTracer();

            var startDownloadTime = DateTime.Now;
            try
            {
                var internalCancellationTokenSource = new CancellationTokenSource();
                using (internalCancellationTokenSource)
                {
                    if (cancellationTokenSource != null)
                    {
                        cancellationTokenSource.Token.Register(() => internalCancellationTokenSource.Cancel(false));
                    }


                    // 如果存在原有下载，那么下载文件需要让局域网下载的放在另一个文件，解决两个线程下载文件冲突
                    var peerToPeerFilePath = GetP2PFilePath(downloadFileEntry);

                    // 同时使用P2P下载和直接下载

                    var downloadTask = straightDownloader.StartDownload(progress, internalCancellationTokenSource);

                    // 这里使用原有下载的进度，忽略P2P下载进度
                    progress = new Progress<DownloadProgress>();
                    var p2pTask = DownloadFromP2P(peerToPeerFilePath, downloadFileEntry, progress,
                        internalCancellationTokenSource.Token);

                    var task = await Task.WhenAny(downloadTask, p2pTask);

                    if (ReferenceEquals(task, downloadTask))
                    {
                        Console.WriteLine($"原有下载完成 {downloadFileEntry.DownloadFile.Name}");
                        // 原先的下载完成了，那么不需要局域网下载
                        internalCancellationTokenSource.Cancel();

                        // 原先的下载方式
                        if (CheckSucceed(task))
                        {
                            // 下载成功，注册资源
                            RegisterResource(downloadFileEntry);
                        }
                    }
                    else
                    {
                        var isP2PSucceed = CheckSucceed(task);
                        if (isP2PSucceed)
                        {
                            try
                            {
                                // 如果P2P下载成功，原先的下载就不需要
                                internalCancellationTokenSource.Cancel();

                                //todo 解决原有下载的文件占用

                                // 移动文件，因为P2P先下载的文件不是用户需要的文件
                                File.Move(peerToPeerFilePath.FullName, downloadFileEntry.DownloadFile.FullName);

                                RegisterResource(downloadFileEntry);

                                p2pDownloadTracer.DownloadFromP2P = true;
                            }
                            catch (Exception)
                            {
                                // 这里不处理异常
                            }
                        }
                        else
                        {
                            task = downloadTask;
                            await task;
                        }
                    }

                    if (task.IsCompleted)
                    {
                        return task.Result;
                    }

                    return DownloadCompletedArgs.GetFailDownloadCompletedArgs();

                    //DownloadFromWebServiceAsync(webService,downloadFileEntry,progress,internalCancellationTokenSource);
                }
            }
            finally
            {
                var file = new FileInfo(downloadFileEntry.DownloadFile.FullName);
                if (file.Exists)
                {
                    p2pDownloadTracer.FileSize = file.Length;
                }

                p2pDownloadTracer.CostTime = DateTime.Now - startDownloadTime;
            }
        }

        /// <summary>
        /// 从P2P下载资源
        /// </summary>
        /// <param name="resourceKey">资源名</param>
        /// <param name="downloadFolder">下载到的文件夹</param>
        /// <param name="downloadFileName">下载的文件名，可以不写，不写将会使用服务器返回的文件名</param>
        /// <param name="httpClient"></param>
        /// <param name="token"></param>
        /// <returns>如返回success表示正在下载，可以通过 processReport 在 <see cref="GetDownloadProcess"/> 方法获取到进度</returns>
        public async Task<(bool success, ProcessReport processReport)> DownloadFileAsync(string resourceKey,
            DirectoryInfo downloadFolder,
            string downloadFileName = null,
            HttpClient httpClient = null,
            CancellationToken token = default)
        {
            // 下载之前判断服务开启
            if (!P2PProvider.P2PProcess.TryStart())
            {
                return (false, default);
            }

            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            var url = $"{P2PProvider.P2PHost}api/Resource/Download";

            var downloadFileInfo = new DownloadFileInfo
            {
                FileId = resourceKey,
                DownloadFolderPath = downloadFolder.FullName,
                FileName = downloadFileName
            };

            var json = JsonConvert.SerializeObject(downloadFileInfo);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (token == default)
            {
                token = CancellationToken.None;
            }

            try
            {
                var message = await httpClient.PostAsync(url, content, token);

                if (message.StatusCode == HttpStatusCode.OK)
                {
                    var str = await message.Content.ReadAsStringAsync();

                    var processReport = Json.Parse<ProcessReport>(str);
                    return (true, processReport);
                }
            }
            catch (Exception)
            {
            }

            return (false, default);
        }

        /// <summary>
        /// 获取下载进度
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="processReport"></param>
        /// <param name="downloadCompletionSource"></param>
        /// <param name="httpClient"></param>
        public void GetDownloadProcess(IProgress<DownloadProgress> progress, ProcessReport processReport,
            TaskCompletionSource<bool> downloadCompletionSource, HttpClient httpClient = null)
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            Task.Run(async () =>
            {
                while (!downloadCompletionSource.Task.IsCompleted)
                {
                    var s = await httpClient.GetStringAsync(
                        $"{P2PProvider.P2PHost}api/Resource/DownloadProcess?id={processReport.Id}");
                    var process = Json.Parse<ProcessReport>(s);
                    progress.Report(new DownloadProgress((long)process.Process, (long)process.MaxProcess));

                    if (processReport.Process < 0)
                    {
                        downloadCompletionSource.SetResult(false);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }

        private static bool CheckSucceed(Task<DownloadCompletedArgs> task)
        {
            return task.IsCompleted && (task.Exception == null && task.Result.IsSucceed);
        }

        /// <summary>
        /// 注册资源
        /// </summary>
        /// <param name="downloadFileEntry"></param>
        private async void RegisterResource(P2PDownloadFileEntry downloadFileEntry)
        {
            var p2PRegister = P2PProvider.P2PRegister;
            await p2PRegister.RegisterResourceAsync(downloadFileEntry.ResourceKey, downloadFileEntry.DownloadFile);
        }

        private FileInfo GetP2PFilePath(P2PDownloadFileEntry downloadFileEntry)
        {
            // 先获取原有文件地址
            var downloadFileDirectory = downloadFileEntry.DownloadFile.Directory;
            //todo 解决重命名
            return new FileInfo(Path.Combine(downloadFileDirectory.FullName,
                downloadFileEntry.DownloadFile.Name + ".P2PDownload"));
        }

        //private async Task<AsyncCompletedEventArgs> DownloadFromWebServiceAsync(WebService webService, P2PDownloadFileEntry downloadFileEntry,
        //    IProgress<DownloadProgress> progress, CancellationTokenSource cancellationTokenSource)
        //{
        //    return
        //        await webService.DownloadFileWithResume(new List<DownloadFileEntry>(){  downloadFileEntry}, progress, cancellationTokenSource.Token, false);
        //}


        //public async Task Download(DownloadEntry downloadEntry, IProgress<ProcessReport> progress = null)
        //{
        //    await DownloadFromP2P(downloadEntry, progress);
        //}

        private async Task<DownloadCompletedArgs> DownloadFromP2P(FileInfo peerToPeerFilePath,
            P2PDownloadFileEntry downloadEntry,
            IProgress<DownloadProgress> progress,
            CancellationToken token)
        {
            var downloadFile = peerToPeerFilePath;
            var downloadFolder = downloadFile.Directory;
            var resourceKey = downloadEntry.ResourceKey;

            return await DownloadFromP2P(resourceKey, downloadFile, downloadFolder, progress, token);
        }

        private async Task<DownloadCompletedArgs> DownloadFromP2P(string resourceKey, FileInfo downloadFile,
            DirectoryInfo downloadFolder, IProgress<DownloadProgress> progress, CancellationToken token)
        {
            var downloadCompletionSource = new TaskCompletionSource<bool>();
            token.Register(() => { downloadCompletionSource.TrySetCanceled(); });

            var p2PProcess = P2PProvider.P2PProcess;
            if (!p2PProcess.TryStart())
            {
                return DownloadCompletedArgs.GetFailDownloadCompletedArgs("P2P没有启动或安装");
            }

            try
            {
                Directory.CreateDirectory(downloadFolder.FullName);

                using (var downloadFileWatcher = new DownloadFileWatcher(downloadFile))
                {
                    var httpClient = new HttpClient();

                    var (success, processReport) =
                        await DownloadFileAsync(resourceKey, downloadFolder, downloadFile.Name, httpClient, token);

                    if (success)
                    {
                        progress.Report(new DownloadProgress((long)processReport.Process,
                            (long)processReport.MaxProcess));

                        //GetDownloadProcess(progress, processReport, downloadCompletionSource, httpClient);

                        var fileDownloadTask = downloadFileWatcher.WaitForFileDownloaded();

                        await Task.WhenAny(fileDownloadTask, downloadCompletionSource.Task);

                        if (fileDownloadTask.IsCompleted)
                        {
                            downloadCompletionSource.SetResult(true);
                            return DownloadCompletedArgs.GetSucceedDownloadCompletedArgs();
                        }

                        if (downloadCompletionSource.Task.IsCanceled)
                        {
                            _ = fileDownloadTask.ContinueWith(_ =>
                            {
                                // 尝试删除文件
                                try
                                {
                                    if (File.Exists(downloadFile.FullName))
                                    {
                                        Debug.WriteLine("因为其他下载完成，删除文件");
                                        File.Delete(downloadFile.FullName);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                }
                            });
                        }
                    }

                    return DownloadCompletedArgs.GetFailDownloadCompletedArgs();
                }
            }
            catch (Exception e)
            {
                // 等待其他方法下载完成
                //await downloadCompletionSource.Task;
                return DownloadCompletedArgs.GetFailDownloadCompletedArgs(e);
            }
        }
    }
}