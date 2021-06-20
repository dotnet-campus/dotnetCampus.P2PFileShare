using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotnetCampusP2PFileShare.Core.Peer;
using DotnetCampusP2PFileShare.Demo.Annotations;

namespace DotnetCampusP2PFileShare.Model
{
    public class NodeModel : INotifyPropertyChanged, IReadOnlyDeviceInfo
    {
        public string NodeCount
        {
            set
            {
                if (value == _nodeCount) return;
                _nodeCount = value;
                OnPropertyChanged();
            }
            get => _nodeCount;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public string Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (value == _deviceName) return;
                _deviceName = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public string DeviceId
        {
            get => _deviceId;
            set
            {
                if (value == _deviceId) return;
                _deviceId = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public string DevicePort
        {
            get => _devicePort;
            set
            {
                if (value == _devicePort) return;
                _devicePort = value;
                OnPropertyChanged();
            }
        }

        private string _deviceId;
        private string _deviceName;
        private string _devicePort;
        private string _nodeCount;
        private string _version;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}