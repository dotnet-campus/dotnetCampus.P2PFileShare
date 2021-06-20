using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetCampusP2PFileShare.Data;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Core.FileStorage
{
    public class FileManager
    {
        public bool TryFindResource(string resourceId, out FolderResource resource)
        {
            resource = null;

            using var fileManagerContext = new FileManagerContext();
            var resourceModel = fileManagerContext.ResourceModel.FirstOrDefault(temp => temp.ResourceId == resourceId);
            if (resourceModel != null)
            {
                resource = ConvertToFolderResource(resourceModel);
                // 校验一下文件存在
                if (CheckResourceExist(resourceModel))
                {
                    return true;
                }

                fileManagerContext.ResourceModel.Remove(resourceModel);
                fileManagerContext.SaveChangesAsync();
                return false;
            }

            return false;

            //if (!TryGetFile(resourceId, null, out _))
            //{
            //    resource = null;
            //    return false;
            //}

            //resource = new FolderResource
            //{
            //    FileResourceList = new List<FileResource>()
            //    {
            //        new FileResource()
            //        {
            //            // 下面是测试代码
            //            FileRelativePath = "",
            //            FileName = "lindexi.zip",
            //            FileSign = "123",
            //            DownloadUrl = "/api/Peer/DownloadFile?resourceId=lindexi.zip"
            //        }
            //    }
            //};
        }

        public bool TryGetFile(string resourceId, string relativePath, out FileInfo file)
        {
            using var fileManagerContext = new FileManagerContext();
            var resourceModel = fileManagerContext.ResourceModel.FirstOrDefault(temp => temp.ResourceId == resourceId);
            if (resourceModel != null)
            {
                // todo 处理文件拼接的安全性
                // 不能让用户输入 relativePath=../..

                var path = resourceModel.LocalPath;
                if (!string.IsNullOrEmpty(relativePath))
                {
                    path = Path.Combine(path, relativePath);
                }

                file = new FileInfo(path);

                if (file.Exists)
                {
                    return true;
                }

                // 文件不存在了，从数据库移除
                fileManagerContext.ResourceModel.Remove(resourceModel);
                fileManagerContext.SaveChanges();
                return false;
            }

            file = null;

            return false;
        }

        public void AddResource(UploadResourceInfo uploadResourceInfo)
        {
            // 假定传入的是文件
            var file = uploadResourceInfo.LocalPath;

            // todo 判断传入的是文件夹
            if (!File.Exists(file))
            {
                return;
            }

            var resourceModel = new ResourceModel
            {
                ResourceId = uploadResourceInfo.ResourceId,
                LocalPath = uploadResourceInfo.LocalPath,
                ResourceName = Path.GetFileName(file)
                //ResourceFileDetail = 
            };
            // todo 后台给文件签名
            //resourceModel.ResourceSign

            using var fileManagerContext = new FileManagerContext();
            // 判断是否已经加入
            var foundResource =
                fileManagerContext.ResourceModel
                    .FirstOrDefault(temp => temp.ResourceId == resourceModel.ResourceId);
            if (foundResource != null)
            {
                // 找到了之前的资源，但是重新注册可能是修改了资源
                if (CheckResourceExist(foundResource))
                {
                    P2PTracer.Info(string.Equals(foundResource.LocalPath, resourceModel.LocalPath)
                        ? $"已存在{resourceModel.ResourceId}资源"
                        : $"已存在{resourceModel.ResourceId}资源，原有{foundResource.LocalPath} 注册 {resourceModel.LocalPath}");

                    return;
                }

                fileManagerContext.ResourceModel.Remove(foundResource);
            }

            P2PTracer.Info($"注册{resourceModel.ResourceId}资源 {resourceModel.LocalPath}");
            fileManagerContext.ResourceModel.Add(resourceModel);
            fileManagerContext.SaveChanges();
        }

        private FolderResource ConvertToFolderResource(ResourceModel resourceModel)
        {
            // 对于单文件，将不会存放文件信息
            if (!CheckFolderResource(resourceModel))
            {
                var resource = new FolderResource
                {
                    // 单文件将不会存在文件夹名
                    //FolderName =
                    FileResourceList = new List<FileResource>
                    {
                        // 就一个文件
                        new FileResource
                        {
                            FileRelativePath = "",
                            FileName = resourceModel.ResourceName,
                            DownloadUrl = CombineDownloadUrl(resourceModel.ResourceId),
                            FileSign = resourceModel.ResourceSign
                        }
                    }
                };

                return resource;
            }
            else
            {
                var fileList = FileDetailParser.Deserialize(resourceModel.ResourceFileDetail);

                var fileResourceList = new List<FileResource>();

                foreach (var file in fileList)
                {
                    var relativePath = Path.GetDirectoryName(file);
                    var fileName = Path.GetDirectoryName(file);
                    var downloadUrl = CombineDownloadUrl(resourceModel.ResourceId, file);

                    fileResourceList.Add(new FileResource
                    {
                        DownloadUrl = downloadUrl,
                        FileName = fileName,
                        FileSign = null,
                        FileRelativePath = relativePath
                    });
                }

                var resource = new FolderResource
                {
                    FolderName = resourceModel.ResourceName,
                    FileResourceList = fileResourceList
                };

                return resource;
            }
        }

        /// <summary>
        /// 判断当前是文件夹资源
        /// </summary>
        /// <param name="resourceModel"></param>
        /// <returns></returns>
        private static bool CheckFolderResource(ResourceModel resourceModel)
        {
            return !string.IsNullOrEmpty(resourceModel.ResourceFileDetail);
        }

        private string CombineDownloadUrl(string resourceId, string relativePath = null)
        {
            resourceId = Uri.EscapeDataString(resourceId);

            var downloadUrl = $"/api/Peer/DownloadFile?resourceId={resourceId}";

            if (!string.IsNullOrEmpty(relativePath))
            {
                relativePath = Uri.EscapeDataString(relativePath);
                downloadUrl = $"{downloadUrl}&relativePath={relativePath}";
            }

            return downloadUrl;
        }

        /// <summary>
        /// 判断资源存在
        /// </summary>
        /// <param name="resourceModel"></param>
        /// <returns></returns>
        private bool CheckResourceExist(ResourceModel resourceModel)
        {
            if (CheckFolderResource(resourceModel))
            {
                return Directory.Exists(resourceModel.LocalPath);
            }

            return File.Exists(resourceModel.LocalPath);
        }
    }
}