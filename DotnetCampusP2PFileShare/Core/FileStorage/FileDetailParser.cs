using System.Collections.Generic;

namespace DotnetCampusP2PFileShare.Core.FileStorage
{
    internal static class FileDetailParser
    {
        public static string Serialize(IEnumerable<string> fileList)
        {
            return string.Join(Separator, fileList);
        }

        public static string[] Deserialize(string resourceFileDetail)
        {
            return resourceFileDetail.Split(Separator);
        }

        private const char Separator = ':';
    }
}