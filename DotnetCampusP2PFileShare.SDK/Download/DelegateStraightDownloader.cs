using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.SDK.Download
{
    public class DelegateStraightDownloader : IStraightDownloader
    {
        /// <inheritdoc />
        public DelegateStraightDownloader(
            Func<IProgress<DownloadProgress>, CancellationToken, Task<AsyncCompletedEventArgs>> downloadDelegate)
        {
            if (ReferenceEquals(downloadDelegate, null)) throw new ArgumentNullException(nameof(downloadDelegate));
            _downloadDelegate = downloadDelegate;
        }

        public async Task<DownloadCompletedArgs> StartDownload(IProgress<DownloadProgress> progress,
            CancellationTokenSource cancellationTokenSource)
        {
            var asyncCompletedEventArgs = await _downloadDelegate(progress, cancellationTokenSource.Token);
            if (asyncCompletedEventArgs.Cancelled)
            {
                return DownloadCompletedArgs.GetAbortDownloadCompletedArgs(asyncCompletedEventArgs.Error);
            }

            if (asyncCompletedEventArgs.Error != null)
            {
                return DownloadCompletedArgs.GetFailDownloadCompletedArgs(asyncCompletedEventArgs.Error);
            }

            return DownloadCompletedArgs.GetSucceedDownloadCompletedArgs();
        }

        private readonly Func<IProgress<DownloadProgress>, CancellationToken, Task<AsyncCompletedEventArgs>>
            _downloadDelegate;
    }

    public class DownloadProgress
    {
        public DownloadProgress(long processProcess, long processMaxProcess)
        {
            
        }
    }
}