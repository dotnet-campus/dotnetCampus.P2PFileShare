using System;
using System.Collections.Generic;
using System.Linq;
using DotnetCampusP2PFileShare.Model;

namespace DotnetCampusP2PFileShare.SDK.Upload
{
    /// <summary>
    /// 第一次启动资源注册
    /// </summary>
    public class FirstResourceRegister : IDisposable
    {
        /// <summary>
        /// 添加资源注册
        /// </summary>
        /// <param name="resourceProvider"></param>
        public void AddResourceProvider(IResourceProvider resourceProvider)
        {
            if (_disposedValue) return;
            ResourceProviderList.Add(resourceProvider);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposedValue;

        private List<IResourceProvider> ResourceProviderList { set; get; } = new List<IResourceProvider>();

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                ResourceProviderList.Clear();
                ResourceProviderList = null;
                _disposedValue = true;
            }
        }

        internal List<UploadResourceInfo> GetResourceInfoList()
        {
            var uploadResourceInfoList = new List<UploadResourceInfo>();
            foreach (var temp in ResourceProviderList.Select(temp => temp.GetUploadResourceInfoList()))
            {
                uploadResourceInfoList.AddRange(temp);
            }

            return uploadResourceInfoList;
        }
    }
}