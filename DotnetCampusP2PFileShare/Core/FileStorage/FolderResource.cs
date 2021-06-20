using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetCampusP2PFileShare.Core.FileStorage
{
    public class FolderResource
    {
        /// <summary>
        /// 文件夹名，可能为空
        /// </summary>
        public string FolderName { set; get; }

        [JsonPropertyName(nameof(FileResourceList))]
        public List<FileResource> FileResourceList { set; get; }
    }
}