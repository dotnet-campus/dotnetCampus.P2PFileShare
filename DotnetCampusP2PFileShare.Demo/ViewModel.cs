using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotnetCampusP2PFileShare.Demo.Annotations;
using DotnetCampusP2PFileShare.Model;

namespace DotnetCampusP2PFileShare.Demo
{
    public class ViewModel : INotifyPropertyChanged
    {
        public NodeModel NodeModel { get; set; } = new NodeModel();

        public FileModel FileModel { get; } = new FileModel();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}