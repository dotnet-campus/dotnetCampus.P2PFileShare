using System.Collections.Generic;
using DotnetCampusP2PFileShare.Model;

namespace DotnetCampusP2PFileShare.SDK.Upload
{
    /// <summary>
    /// 提供资源
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// 获取需要上传资源
        /// </summary>
        /// <returns></returns>
        List<UploadResourceInfo> GetUploadResourceInfoList();
    }
}