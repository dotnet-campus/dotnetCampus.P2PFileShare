using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using DotnetCampusP2PFileShare.Model;

namespace DotnetCampusP2PFileShare.Demo
{
    /// <summary>
    /// NodePage.xaml 的交互逻辑
    /// </summary>
    public partial class NodePage : UserControl
    {
        public NodePage()
        {
            InitializeComponent();

            NodeModel = (NodeModel)DataContext;
        }

        public NodeModel NodeModel { get; }

        private void DeviceName_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NodeModel.DeviceName))
            {
                return;
            }

            var url = $"http://127.0.0.1:5125/api/Device/SetDeviceName?name={NodeModel.DeviceName}";
            var httpClient = new HttpClient();
            httpClient.GetStringAsync(url);
        }
    }
}