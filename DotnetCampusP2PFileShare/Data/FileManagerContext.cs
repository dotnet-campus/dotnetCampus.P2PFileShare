using System;
using System.IO;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.P2PLogging;

namespace DotnetCampusP2PFileShare.Data
{
    public class FileManagerContext : DbContext
    {
        public static string DataFile { get; } =
            Path.Combine(AppConfiguration.Current.ConfigurationFolder, "FileManger_1_0_1.db");

        public DbSet<ResourceModel> ResourceModel { get; set; }

        /// <summary>
        /// 创建数据库
        /// </summary>
        public static void Migrate()
        {
            try
            {
                var file = DataFile;
                if (!File.Exists(file))
                {
                    var fileManagerContext = new FileManagerContext();
                    using (fileManagerContext)
                    {
                        fileManagerContext.Database.Migrate();
                    }
                }
            }
            catch (Exception e)
            {
                P2PTracer.Report(e, "CreateDatabase");
                CopyDataFile();
            }
        }

        private static void CopyDataFile()
        {
            // 备用方案，当无法迁移创建则复制已经创建好的数据库
            try
            {
                if (File.Exists(DataFile))
                {
                    File.Delete(DataFile);
                }

                var file = Path.Combine(AppConfiguration.Current.ExeFolder, "FileManger.db");
                File.Copy(file, DataFile);
            }
            catch (Exception e)
            {
                // 如果复制都失败，那么软件就退出
                P2PTracer.Report(e, "CopyDatabase");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                Environment.Exit(-1);
            }
        }

        //public DbSet<DotnetCampusP2PFileShare.Model.NodeModel> NodeModel { get; set; }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var file = DataFile;
            optionsBuilder
                .UseSqlite($"Data Source={file}");
        }
    }
}