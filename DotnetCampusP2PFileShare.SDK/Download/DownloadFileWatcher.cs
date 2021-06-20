using System;
using System.IO;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.SDK.Download
{
    internal class DownloadFileWatcher : IDisposable
    {
        /// <inheritdoc />
        public DownloadFileWatcher(FileInfo downloadFile)
        {
            DownloadFile = downloadFile;
            var fileSystemWatcher = new FileSystemWatcher(downloadFile.DirectoryName)
            {
                EnableRaisingEvents = true
            };
            fileSystemWatcher.Changed += FileSystemWatcher_Created;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            _fileSystemWatcher = fileSystemWatcher;

            var taskCompletionSource = new TaskCompletionSource<bool>();
            _taskCompletionSource = taskCompletionSource;

            //var dispatcherAsyncOperation = DispatcherAsyncOperation.Create(out var reportResult);
            //_reportResult = reportResult;
            //_dispatcherAsyncOperation = dispatcherAsyncOperation;
        }

        public FileInfo DownloadFile { get; }

        public async Task WaitForFileDownloaded()
        {
            await _taskCompletionSource.Task;
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
            _taskCompletionSource.TrySetCanceled();
        }

        //private readonly Action<Exception> _reportResult;
        //private readonly DispatcherAsyncOperation _dispatcherAsyncOperation;
        private readonly FileSystemWatcher _fileSystemWatcher;

        private readonly TaskCompletionSource<bool> _taskCompletionSource;

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.FullPath == DownloadFile.FullName)
            {
                _taskCompletionSource.SetResult(true);

                _fileSystemWatcher.Dispose();
            }
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Created) != 0)
            {
                if (e.FullPath == DownloadFile.FullName)
                {
                    _taskCompletionSource.SetResult(true);

                    _fileSystemWatcher.Dispose();
                }
            }
        }
    }
}