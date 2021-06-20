using System.IO;

namespace DotnetCampusP2PFileShare.SDK.Context
{
    public class DownloadEntry
    {
        public FileInfo DownloadFile { set; get; }

        public string ResourceKey { set; get; }
    }
}