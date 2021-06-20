using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotnetCampusP2PFileShare.Demo.Annotations;

namespace DotnetCampusP2PFileShare.Model
{
    public class FileModel : INotifyPropertyChanged
    {
        public string ResourceId
        {
            get => _resourceId;
            set
            {
                if (value == _resourceId) return;
                _resourceId = value;
                OnPropertyChanged();
            }
        }

        public string File
        {
            set
            {
                if (value == _file) return;
                _file = value;
                OnPropertyChanged();
            }
            get => _file;
        }


        public string Folder
        {
            get => _folder;
            set
            {
                if (value == _folder) return;
                _folder = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private string _file;
        private string _folder;
        private string _resourceId;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}