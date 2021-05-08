using System.Net.NetworkInformation;

namespace AltNetworkUtility.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
        private string? _Name;
        public string? Name { get => _Name; set => SetProperty(ref _Name, value); }

        private string? _Icon;
        public string? Icon { get => _Icon; set => SetProperty(ref _Icon, value); }

        private bool _IsUp = false;
        public bool IsUp { get => _IsUp; set => SetProperty(ref _IsUp, value); }

        public NetworkInterfaceViewModel(NetworkInterface networkInterface)
        {
            Name = networkInterface.Name;
            Icon = networkInterface.NetworkInterfaceType switch
            {
                NetworkInterfaceType.Ethernet => "network",
                NetworkInterfaceType.Wireless80211 => "wifi",
                _ => "questionmark.diamond"
            };
            IsUp = networkInterface.OperationalStatus == OperationalStatus.Up;
        }
    }
}
