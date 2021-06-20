using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetCampusP2PFileShare.Model
{
    public class NodeModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { set; get; }

        /// <summary>
        /// 设备的链接
        /// </summary>
        public string NodeUrl { set; get; }

        /// <summary>
        /// 最近更新设备的时间
        /// </summary>
        public DateTime LastUpdate { set; get; }
    }
}