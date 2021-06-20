namespace DotnetCampusP2PFileShare.Model
{
    /// <summary>
    /// 上传的资源信息
    /// </summary>
    public class UploadResourceInfo
    {
        /// <summary>
        /// 本机的路径，使用绝对路径
        /// </summary>
        public string LocalPath { set; get; }

        /// <summary>
        /// 资源的 ID 值
        /// </summary>
        public string ResourceId { set; get; }

        /// <summary>
        /// 文件的Md5校验，可选
        /// </summary>
        public string FileMd5Hash { set; get; }

        /// <summary>
        /// 文件大小，可选
        /// </summary>
        public long FileSize { set; get; }
    }
}