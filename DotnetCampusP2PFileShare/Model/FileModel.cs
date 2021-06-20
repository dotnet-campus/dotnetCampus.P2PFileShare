using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetCampusP2PFileShare.Core.FileStorage;
using Microsoft.EntityFrameworkCore;

namespace DotnetCampusP2PFileShare.Model
{
    [Index(nameof(ResourceId))]
    public class ResourceModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { set; get; }

        /// <summary>
        /// 资源的标识符，对于单个文件，默认是 MD5 的值
        /// </summary>
        [StringLength(300)]
        public string ResourceId { set; get; }

        /// <summary>
        /// 资源名
        /// </summary>
        [StringLength(300)]
        public string ResourceName { set; get; }

        /// <summary>
        /// 资源所在本地的绝对路径
        /// </summary>
        [StringLength(260)]
        public string LocalPath { set; get; }

        /// <summary>
        /// 资源的签名
        /// </summary>
        [StringLength(300)]
        public string ResourceSign { set; get; }

        /// <summary>
        /// 资源文件的信息，用于资源是文件夹的情况，此时在 <see cref="LocalPath"/> 存放的是文件夹的绝对路径，而在 <see cref="ResourceFileDetail"/> 存放的是各个文件的信息。序列化方式请参阅 <see cref="FileDetailParser"/> 的实现
        /// </summary>
        /// todo 处理文件夹里面存在太多文件的情况，是否允许动态修改文件夹内容的情况
        public string ResourceFileDetail { set; get; }
    }
}