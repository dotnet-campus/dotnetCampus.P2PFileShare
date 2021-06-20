using System;
using System.Threading.Tasks;

namespace DotnetCampusP2PFileShare.Core.Application
{
    /// <summary>
    /// 运行时的测试逻辑
    /// </summary>
    public static class RunTimeTest
    {
        public static void AddTest(Action action)
        {
            if (CanUseTest)
            {
                action();
            }
        }

        public static async Task AddTest(Func<Task> action)
        {
            if (CanUseTest)
            {
                await action();
            }
        }

        private static bool CanUseTest { get; } = true;
    }
}