using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetCampusP2PFileShare.Core
{
    public static class Container
    {
        public static void RegisterServiceProvider(IServiceProvider buildServiceProvider)
        {
            BuildServiceProvider = buildServiceProvider;
        }

        public static T Get<T>()
        {
            if (ValueList.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }

            if (LazyDictionary.TryGetValue(typeof(T), out var lazy))
            {
                return (T)lazy.Value;
            }

            return default;
        }

        public static void Set<T>(T value)
        {
            ValueList[typeof(T)] = value;
        }

        public static void Set<T>(Func<T> creator)
        {
            var lazy = new Lazy<object>(() => creator(), LazyThreadSafetyMode.ExecutionAndPublication);
            LazyDictionary[typeof(T)] = lazy;
        }

        public static T GetService<T>()
        {
            return BuildServiceProvider.GetService<T>();
        }

        /// <summary>
        /// 延迟加载的字典
        /// </summary>
        private static ConcurrentDictionary<Type, Lazy<object>> LazyDictionary { get; } =
            new ConcurrentDictionary<Type, Lazy<object>>();

        private static ConcurrentDictionary<Type, object> ValueList { get; } = new ConcurrentDictionary<Type, object>();

        private static IServiceProvider BuildServiceProvider { get; set; }

        //public static ILoggerFactory LoggerFactory { get; } = Microsoft.Extensions.Logging.LoggerFactory.Create(
        //    builder => builder
        //        .AddConsole()
        //        .AddDebug()
        //        .AddFile(
        //        Path.Combine(AppConfiguration.Current.ExeFolder, "logs", $"database {DateTime.Now:yyMMddhhmmss}.txt"),
        //        fileSizeLimitBytes: 1024 * 1024 * 30));
    }
}