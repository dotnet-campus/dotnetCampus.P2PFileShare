using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DotnetCampusP2PFileShare.SDK.Install
{
    /// <summary>
    /// 寻找对应的P2P文件
    /// </summary>
    internal class P2PFileFinder
    {
        public bool TryFindP2PFile(out FileInfo file)
        {
            file = null;

            // 判断本地文件是否存在  

            var dotnetCampusP2PFileShareFolder = GetDotnetCampusP2PFileShareFolder();

            if (!Directory.Exists(dotnetCampusP2PFileShareFolder))
            {
                return false;
            }

            var installFolder = new DirectoryInfo(dotnetCampusP2PFileShareFolder);
            var maxVersion = new Version(0, 0, 0);
            var dotnetCampusP2PFileShareFile = "";

            foreach (var folder in installFolder.GetDirectories())
            {
                if (TryParseVersion(folder.Name, out var version))
                {
                    if (version > maxVersion)
                    {
                        maxVersion = version;
                        dotnetCampusP2PFileShareFile = Path.Combine(folder.FullName, "DotnetCampusP2PFileShare.exe");
                    }
                }
            }

            if (!string.IsNullOrEmpty(dotnetCampusP2PFileShareFile) && File.Exists(dotnetCampusP2PFileShareFile))
            {
                file = new FileInfo(dotnetCampusP2PFileShareFile);
                return true;
            }

            return false;
        }

        public string GetDotnetCampusP2PFileShareFolder()
        {
            var dotnetCampusP2PFileShareFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DotnetCampus", "DotnetCampusP2PFileShare");
            return dotnetCampusP2PFileShareFolder;
        }

        private bool TryParseVersion(string folder, out Version version)
        {
            version = default;
            var regex = new Regex(@"DotnetCampusP2PFileShare_([\S\s]*)");
            var match = regex.Match(folder);
            if (match.Success)
            {
                return Version.TryParse(match.Groups[1].Value, out version);
            }

            return false;
        }
    }
}