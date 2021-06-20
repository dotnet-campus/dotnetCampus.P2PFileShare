using System.IO;

namespace DotnetCampusP2PFileShare.SDK.Context
{
    public class P2PDownloadFileEntry
    {
        /// <summary>
        /// 全网唯一标识文件
        /// </summary>
        public string ResourceKey { get; set; }

        ///// <summary>
        ///// 下载地址
        ///// </summary>
        //public Uri DownloadUri { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public FileInfo DownloadFile { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件Hash
        /// </summary>
        public string FileMd5 { get; set; }
    }
}