using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetCampusP2PFileShare.Core.Context;

namespace DotnetCampusP2PFileShare.SDK.Install
{
    internal class P2PProcess
    {
        /// <param name="p2PProvider"></param>
        /// <inheritdoc />
        public P2PProcess(P2PProvider p2PProvider)
        {
            P2PProvider = p2PProvider;
        }

        public P2PProvider P2PProvider { get; }

        public bool? CanInstall { set; get; }

        /// <summary>
        /// 尝试启动，如果此时的 P2P 没有下载，那么启动失败，如果已经启动了，那么什么都不做
        /// </summary>
        /// <returns></returns>
        public bool TryStart()
        {
            if (CheckRunning())
            {
                return true;
            }

            if (CheckStarting())
            {
                return false;
            }

            LastStart = DateTime.Now;

            var p2PSettings = P2PProvider.P2PSettings;
            var p2PBoot = p2PSettings.P2PBoot;

            // 启动一下
            if (p2PBoot.StartP2P())
            {
                return true;
            }

            // 启动失败，尝试安装
            TryInstallP2PFile();

            return false;
        }

        private async Task RegisterResource()
        {
            var p2PSettings = P2PProvider.P2PSettings;
            var uploadResourceInfoList = p2PSettings.FirstResourceRegister.GetResourceInfoList();

            var p2PRegister = P2PProvider.P2PRegister;

            foreach (var uploadResourceInfo in uploadResourceInfoList)
            {
                await p2PRegister.RegisterResourceAsync(uploadResourceInfo);
            }
        }

        /// <summary>
        /// 判断是否正在启动
        /// </summary>
        /// <returns></returns>
        private bool CheckStarting()
        {
            // 一分钟内启动过的就不继续启动
            return DateTime.Now - LastStart < TimeSpan.FromMinutes(1);
        }

        private static DateTime LastStart { set; get; }

        private IP2PInstallAuthority P2PInstallAuthority => P2PProvider.P2PSettings.P2PInstallAuthority;

        private async Task<bool> CheckAuthority()
        {
            if (CanInstall == false)
            {
                // 没有权限下载，可能是系统不满足
                return false;
            }

            if (!CheckEnvironment())
            {
                return false;
            }

            if (P2PInstallAuthority != null)
            {
                if (!await P2PInstallAuthority.CheckAuthorityAsync())
                {
                    // 没有权限下载
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断当前环境是否支持安装
        /// </summary>
        /// <returns></returns>
        private bool CheckEnvironment()
        {
            //win7 非sp1的滚
            // 暂时只给win10用
            var win10 = Environment.OSVersion.Version >= new Version(6, 2);

            return win10;
        }

        /// <summary>
        /// 是否已经开始调用下载P2P服务
        /// </summary>
        private static bool IsStartedInstallP2PProcess { set; get; }

        private async void TryInstallP2PFile()
        {
            // 不需要考虑多线程问题
            if (IsStartedInstallP2PProcess)
            {
                return;
            }

            IsStartedInstallP2PProcess = true;

            // 缓存权限
            if (!await CheckAuthority())
            {
                CanInstall = false;
                return;
            }

            try
            {
                var p2PSettings = P2PProvider.P2PSettings;
                var p2PBoot = p2PSettings.P2PBoot;
                var success = await p2PBoot.TryInstallP2PFile();

                // 安装成功注册资源
                if (success)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    if (!CheckRunning())
                    {
                        p2PBoot.StartP2P();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10));

                    // 首次注入资源
                    await RegisterResource();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            //// 展开一下文件
            //var tempFile = Path.GetTempFileName();
            ////File.WriteAllBytes(tempFile, Resource.DotnetCampusP2PFileShare_1_0_0);

            //// 解压
            //ZipFile.ExtractToDirectory(tempFile, DotnetCampusP2PFileShareFolder);

            //StartP2P(DotnetCampusP2PFileShareFile);
        }


        private bool CheckRunning()
        {
            return FindExistByMutex();
        }

        private bool FindExistByMutex()
        {
            return Mutex.TryOpenExisting(Const.LockId, out _);
        }
    }
}