using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.SDK.Install
{
    internal class P2PBoot : IP2PBoot
    {
        /// <inheritdoc />
        public P2PBoot(P2PSettings p2PSettings)
        {
            P2PSettings = p2PSettings;
        }


        public bool StartP2P()
        {
            var p2PFileFinder = new P2PFileFinder();
            if (p2PFileFinder.TryFindP2PFile(out var file))
            {
                if (file.Exists)
                {
                    StartP2P(file.FullName);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> TryInstallP2PFile()
        {
            return false;
        }

        private P2PSettings P2PSettings { get; }


        private static void StartP2P(string DotnetCampusP2PFileShareFile)
        {
            var processStartInfo = new ProcessStartInfo(DotnetCampusP2PFileShareFile)
            {
                CreateNoWindow = true
            };

            Process.Start(processStartInfo);
        }
    }
}