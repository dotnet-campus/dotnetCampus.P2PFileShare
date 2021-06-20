using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.SDK.Download
{
    public interface IStraightDownloader
    {
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        Task<DownloadCompletedArgs> StartDownload(IProgress<DownloadProgress> progress,
            CancellationTokenSource cancellationTokenSource);
    }

    public class DownloadCompletedArgs
    {
        /// <inheritdoc />
        private DownloadCompletedArgs(bool isSucceed, bool isAbort, Exception exception, string message = "")
        {
            IsSucceed = isSucceed;
            IsAbort = isAbort;
            Exception = exception;
            Message = message;
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        public bool IsSucceed { get; }

        /// <summary>
        /// 下载中断，如参数判断不正确
        /// </summary>
        public bool IsAbort { get; }

        /// <summary>
        /// 下载失败，如网络失败
        /// </summary>
        public bool IsFail => !IsSucceed;

        public string Message { get; }

        public Exception Exception { get; }

        public static DownloadCompletedArgs GetSucceedDownloadCompletedArgs()
        {
            return new DownloadCompletedArgs(true, false, null);
        }

        public static DownloadCompletedArgs GetAbortDownloadCompletedArgs(Exception exception = null)
        {
            return new DownloadCompletedArgs(false, true, exception);
        }

        public static DownloadCompletedArgs GetFailDownloadCompletedArgs(Exception exception = null)
        {
            return new DownloadCompletedArgs(false, false, exception);
        }

        public static DownloadCompletedArgs GetFailDownloadCompletedArgs(string message)
        {
            return new DownloadCompletedArgs(false, false, null, message);
        }
    }
}