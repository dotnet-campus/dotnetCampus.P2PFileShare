using System;
using DotnetCampusP2PFileShare.Core.Net;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.Downloader
{
    /// <summary>
    /// 资源下载器
    /// </summary>
    public class ResourceDownloader
    {
        /// <inheritdoc />
        public ResourceDownloader(PeerToPeerDownloader peerToPeerDownloader, ProcessToken processReport)
        {
            _processReport = processReport;
            PeerToPeerDownloader = peerToPeerDownloader;
        }

        public PeerToPeerDownloader PeerToPeerDownloader { get; }

        public void Download(DownloadFileInfo downloadFileInfo)
        {
            // 下载需要同时从源下载和从局域网其他设备下载

            DownloadFromPeerToPeer(downloadFileInfo);
        }

        private readonly ProcessToken _processReport;

        /// <summary>
        /// 从 P2P 下载
        /// </summary>
        /// <param name="downloadFileInfo"></param>
        private async void DownloadFromPeerToPeer(DownloadFileInfo downloadFileInfo)
        {
            _processReport.SetProcess(0, "从 P2P 下载");
            try
            {
                await PeerToPeerDownloader.Download(downloadFileInfo);
            }
            catch (Exception e)
            {
                _processReport.SetFail(e.ToString());
                P2PTracer.Report(e, EventId.DotnetCampusP2PFileShareDownloadException);
            }
        }
    }
}