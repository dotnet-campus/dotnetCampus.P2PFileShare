using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Newtonsoft.Json;
using DotnetCampusP2PFileShare.Core.Context;
using DotnetCampusP2PFileShare.Core.Peer;

namespace DotnetCampusP2PFileShare.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //ResourceIdText.Text = Guid.NewGuid().ToString("N");

            ViewModel = (ViewModel) DataContext;

            Init();

            //PageControl = PageControl.Resource;
        }

        public static readonly DependencyProperty PageControlProperty = DependencyProperty.Register(
            "PageControl", typeof(PageControl), typeof(MainWindow), new PropertyMetadata(default(PageControl)));

        public ViewModel ViewModel { get; }

        public PageControl PageControl
        {
            get => (PageControl) GetValue(PageControlProperty);
            set => SetValue(PageControlProperty, value);
        }

        private void Init()
        {
            // 判断进程存在
            if (!Process.GetProcesses()
                .Any(temp => temp.ProcessName.Equals("DotnetCampusP2PFileShare", StringComparison.OrdinalIgnoreCase)))
            {
                if (File.Exists("DotnetCampusP2PFileShare.exe"))
                {
                    Process.Start(Path.GetFullPath("DotnetCampusP2PFileShare.exe"));
                }
            }

            Task.Run(GetDevice);
        }

        private async Task GetDevice()
        {
            var url = $"http://127.0.0.1:{Const.DefaultPort}/";

            while (true)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var message = await httpClient.GetStringAsync(url + "api/Device/DeviceInfo");
                        var deviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(message);

                        await Dispatcher.InvokeAsync(() =>
                        {
                            var nodeModel = ViewModel.NodeModel;
                            nodeModel.DeviceId = deviceInfo.DeviceId;
                            nodeModel.DeviceName = deviceInfo.DeviceName;
                            nodeModel.DevicePort = deviceInfo.DevicePort;
                            nodeModel.Version = deviceInfo.Version;
                        });
                    }

                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }

            while (true)
            {
                using (var httpClient = new HttpClient())
                {
                    var message = await httpClient.GetStringAsync(url + "api/Device/ConnectDeviceCount");

                    await Dispatcher.InvokeAsync(() => { ViewModel.NodeModel.NodeCount = message; });
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }

        //public static string ResourceId { set; get; }

        private void PickFile_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void OnPageClosed(object sender, EventArgs e)
        {
            ViewModel.FileModel.ResourceId = "";
            ViewModel.FileModel.File = "";
            PageControl = PageControl.Node;
        }

        private void Download_OnClick(object sender, RoutedEventArgs e)
        {
            PageControl = PageControl.Download;
        }

        private void Upload_OnClick(object sender, RoutedEventArgs e)
        {
            PageControl = PageControl.Upload;
        }

        private async void Button_OnClick(object sender, RoutedEventArgs e)
        {
            //var cloudResourceService = new CloudResourceService();
            //var progress = new Progress<DownloadProgress>();
            //progress.ProgressChanged += (o, downloadProgress) => { Console.WriteLine(downloadProgress); };
            //await cloudResourceService.DownloadAsync(OnlineResourceKey.GeographyTextures, progress,
            //    (b, s, arg) => true);
        }
    }

    public enum PageControl
    {
        Node,
        Download,
        Upload,
        Resource
    }

    public class PageControlToVisibilityConvert : IValueConverter
    {
        public PageControl PageControl { get; set; }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PageControl.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}