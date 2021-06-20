using System;
using System.IO;

namespace DotnetCampusP2PFileShare.Core.FileStorage.Signature
{
    public class FileSignature
    {
        /// <summary>
        /// 文件签名
        /// </summary>
        public string GetMd5Sign(FileInfo file)
        {
            return Md5SecurityUtility.GetMd5HashFromFile(file.FullName);
        }

        /// <summary>
        /// 判断文件签名
        /// </summary>
        /// <param name="file"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        public bool CheckMd5Sign(FileInfo file, string md5)
        {
            return GetMd5Sign(file).Equals(md5, StringComparison.OrdinalIgnoreCase);
        }
    }
}