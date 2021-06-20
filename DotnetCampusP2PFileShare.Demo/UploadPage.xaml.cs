using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.SDK;

namespace DotnetCampusP2PFileShare.Demo
{
    /// <summary>
    /// UploadPage.xaml 的交互逻辑
    /// </summary>
    public partial class UploadPage : UserControl
    {
        public UploadPage()
        {
            InitializeComponent();

            FileModel = (FileModel) DataContext;
        }

        public FileModel FileModel { get; }

        public event EventHandler PageClosed;

        private async void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(FileModel.ResourceId))
                {
                    FileModel.ResourceId = Guid.NewGuid().ToString("N");
                }

                // 请将文件替换为你自己本机的文件
                var file = FileModel.File;
                if (string.IsNullOrEmpty(file) || !File.Exists(file))
                {
                    MessageBox.Show("找不到注册到上传服务的文件，请输入文件绝对路径");
                    return;
                }

                var p2PProvider = new P2PProvider();
                var p2PRegister = p2PProvider.P2PRegister;
                await p2PRegister.RegisterResourceAsync(FileModel.ResourceId, new FileInfo(file));
                MessageBox.Show("本地服务已经记录上传文件");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

            OnPageClosed();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            OnPageClosed();
        }

        protected virtual void OnPageClosed()
        {
            PageClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}