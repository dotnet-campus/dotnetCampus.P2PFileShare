using System;
using System.IO;
using System.Reflection;
using dotnetCampus.Configurations;
using dotnetCampus.Configurations.Core;
using DotnetCampusP2PFileShare.Core.Peer;

namespace DotnetCampusP2PFileShare
{
    /// <summary>
    /// 应用设置
    /// </summary>
    public class AppConfiguration
    {
        /// <inheritdoc />
        private AppConfiguration()
        {
            ExeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ConfigurationFolder = Path.Combine(dataFolder, "DotnetCampus", "DotnetCampusP2PFileShare", "Data");
            Directory.CreateDirectory(ConfigurationFolder);

            var fileConfigurationRepo =
                ConfigurationFactory.FromFile(Path.Combine(ConfigurationFolder, "AppConfiguration.fkv"));
            var appConfigurator = fileConfigurationRepo.CreateAppConfigurator();
            AppConfigurator = appConfigurator;

            CurrentDeviceInfo = DeviceInfo.CreateDeviceInfo(appConfigurator.Of<DeviceConfiguration>());
        }

        public static AppConfiguration Current { get; } = new AppConfiguration();

        public IAppConfigurator AppConfigurator { get; }
        public IReadOnlyDeviceInfo CurrentDeviceInfo { private set; get; }

        /// <summary>
        /// 应用所在的文件夹
        /// </summary>
        public string ExeFolder { get; }

        /// <summary>
        /// 配置文件
        /// </summary>
        public string ConfigurationFolder { get; }

        //public AppConfiguration Current { get; } = new AppConfiguration();

        public void SetDeviceName(string name)
        {
            // 不考虑多线程问题，很少有用户不断设置
            var deviceConfiguration = AppConfigurator.Of<DeviceConfiguration>();
            deviceConfiguration.DeviceName = name;

            var deviceInfo = DeviceInfo.CreateDeviceInfo(AppConfigurator.Of<DeviceConfiguration>());
            deviceInfo.DevicePort = CurrentDeviceInfo.DevicePort;
            CurrentDeviceInfo = deviceInfo;
        }
    }
}