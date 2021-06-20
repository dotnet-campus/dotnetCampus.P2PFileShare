namespace DotnetCampusP2PFileShare.Core.Context
{
    public static class Const
    {
        public const string LockId = "5742D257-CCCC-4F7A-9191-6362609C459D";

        /// <summary>
        /// 默认打开端口，如修改端口可能出现兼容性问题，也就是旧版本的P2P服务找不到本机服务
        /// 如果修改端口，请重新编译 RegisterFirewall 项目，移动编译后文件
        /// </summary>
        public const int DefaultPort = 59091;
    }
}