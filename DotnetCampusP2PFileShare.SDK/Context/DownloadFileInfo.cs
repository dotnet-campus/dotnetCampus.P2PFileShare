namespace DotnetCampusP2PFileShare.Model
{
    /// <summary>
    /// 下载的文件信息
    /// </summary>
    public class DownloadFileInfo
    {
        /// <summary>
        /// 下载文件的 id 号
        /// </summary>
        public string FileId { set; get; }

        /// <summary>
        /// 本地下载保存的文件夹
        /// </summary>
        public string DownloadFolderPath { set; get; }

        /// <summary>
        /// 下载的文件名，如果不知道可以为空
        /// </summary>
        public string FileName { set; get; }
    }
}