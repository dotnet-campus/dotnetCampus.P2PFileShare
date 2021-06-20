using System;
using DotnetCampusP2PFileShare.SDK.Context;
using DotnetCampusP2PFileShare.SDK.Install;
using DotnetCampusP2PFileShare.SDK.Upload;

namespace DotnetCampusP2PFileShare.SDK
{
    /// <summary>
    /// 配置
    /// </summary>
    public class P2PSettings
    {
        /// <inheritdoc />
        public P2PSettings()
        {
            _p2PBoot = new P2PBoot(this);
        }

        /// <summary>
        /// 第一次加载时的资源注册
        /// </summary>
        public FirstResourceRegister FirstResourceRegister { get; } = new FirstResourceRegister();

        ///// <summary>
        ///// 尝试找到对应的文件
        ///// </summary>
        //public IP2PFileFinder P2PFileFinder
        //{
        //    set => _p2PFileFinder = value ?? throw new ArgumentNullException();
        //    get => _p2PFileFinder;
        //}

        /// <summary>
        /// 用于判断P2P是否有权限安装，为空默认有权限
        /// </summary>
        public IP2PInstallAuthority P2PInstallAuthority { set; get; }

        /// <summary>
        /// 用于启动P2P应用
        /// </summary>
        public IP2PBoot P2PBoot
        {
            set => _p2PBoot = value ?? throw new ArgumentNullException();
            get => _p2PBoot;
        }

        private IP2PBoot _p2PBoot;
    }
}