using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotnetCampusP2PFileShare.Core.FileStorage.Signature
{
    public static class Md5SecurityUtility
    {
        /// <summary>
        /// Md5加密文件
        /// </summary>
        /// <param name="filePath">带加密文件路径</param>
        /// <returns>Md5值</returns>
        public static string GetMd5HashFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    var hashBytes = md5.ComputeHash(fileStream);

                    var stringBuilder = new StringBuilder();

                    //string.Concat(hashBytes.Select(temp=>temp.ToString("x2"))

                    for (var idx = 0; idx < hashBytes.Length; idx++)
                    {
                        stringBuilder.Append(hashBytes[idx].ToString("x2"));
                    }

                    return stringBuilder.ToString();
                }
            }
        }

        /// <summary>
        /// 将字符串进行32位Md5加密
        /// </summary>
        /// <param name="valueToEncrypt">待加密字符串</param>
        /// <returns>Md5值</returns>
        public static string Encrypt32(string valueToEncrypt)
        {
            if (null == valueToEncrypt)
            {
                throw new ArgumentNullException("valueToEncrypt");
            }

            var bytesToEncrypt = Encoding.UTF8.GetBytes(valueToEncrypt);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var encryptedBytes = md5.ComputeHash(bytesToEncrypt);

                var stringBuilder = new StringBuilder();

                for (var idx = 0; idx < encryptedBytes.Length; idx++)
                {
                    stringBuilder.Append(encryptedBytes[idx].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
    }
}