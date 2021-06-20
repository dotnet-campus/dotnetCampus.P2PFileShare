namespace DotnetCampusP2PFileShare.Core.FileStorage
{
    public class FileResource
    {
        /// <summary>
        /// 文件相对于文件夹的路径，这个属性是因为减少文件放在文件夹里面的层次
        /// </summary>
        public string FileRelativePath { set; get; }

        /// <summary>
        /// 文件下载链接，这是相对于本机的路径，也就是不包含对应的 Ip 的相对路径
        /// </summary>
        /// 不包含 IP 或域名的作用是本机给其他设备所看到的 IP 会有不同，无法知道其他设备通过哪个 IP 访问到本机
        /// 如本机的 IP 为 172.169.2.19 和 无线网 172.18.134.16 此时有一个设备进行访问，是不能确定他需要用到的是哪个 IP 才对
        public string DownloadUrl { set; get; }

        /// <summary>
        /// 文件签名
        /// </summary>
        public string FileSign { set; get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { set; get; }
    }
}