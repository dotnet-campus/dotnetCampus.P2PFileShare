using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DotnetCampusP2PFileShare.Model;
using DotnetCampusP2PFileShare.SDK;

namespace DotnetCampusP2PFileShare.Demo
{
    /// <summary>
    /// DownloadPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPage : UserControl
    {
        public DownloadPage()
        {
            InitializeComponent();

            FileModel = (FileModel)DataContext;
        }

        public FileModel FileModel { get; }

        public event EventHandler PageClosed;

        private async void Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FileModel.ResourceId))
            {
                MessageBox.Show("请输入下载的文件标识");
                return;
            }

            await Download();

            MessageBox.Show("后台开始下载文件");

            OnPageClosed();
        }

        private async Task Download()
        {
            if (string.IsNullOrEmpty(FileModel.ResourceId))
            {
                return;
            }

            if (string.IsNullOrEmpty(FileModel.Folder))
            {
                FileModel.Folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            var p2PProvider = new P2PProvider();
            var p2PDownloader = p2PProvider.P2PDownloader;
            await p2PDownloader.DownloadFileAsync(FileModel.ResourceId, new DirectoryInfo(FileModel.Folder));

            //var httpClient = new HttpClient();

            //var url = "http://127.0.0.1:5125/api/Resource/Download";


            //var directory = FolderAddress.Text;

            //var downloadFileInfo = new DownloadFileInfo()
            //{
            //    DownloadFolderPath = directory,
            //    FileId = FileModel.ResourceId
            //};


            //var json = JsonConvert.SerializeObject(downloadFileInfo);

            //var content = new StringContent(json, Encoding.UTF8, "application/json");

            //var message = await httpClient.PostAsync(url, content);
            //if (message.StatusCode == HttpStatusCode.OK)
            //{

            //}
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