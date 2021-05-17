using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Linq;

using AltNetworkUtility.Models;
using AltNetworkUtility.Services;
using AltNetworkUtility.Tabs;

using Xamarin.Forms;

namespace AltNetworkUtility.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
        INetworkInterfacesService NetworkInterfacesService;

        public string DisplayName
        {
            get
            {
                if (LocalizedDisplayName != null && Name != null)
                    return $"{LocalizedDisplayName} ({Name})";

                if (LocalizedDisplayName != null)
                    return LocalizedDisplayName;

                if (Name != null)
                    return Name;

                return "(unknown)";
            }
        }

        private string? _Icon;
        public string? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        private string? _IPAddresses;
        public string? IPAddresses
        {
            get => _IPAddresses;
            set => SetProperty(ref _IPAddresses, value);
        }

        private bool _IsUp = false;
        public bool IsUp
        {
            get => _IsUp;
            set => SetProperty(ref _IsUp, value);
        }

        private string? _LocalizedDisplayName;
        public string? LocalizedDisplayName
        {
            get => _LocalizedDisplayName;
            set
            {
                SetProperty(ref _LocalizedDisplayName, value);
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private string? _Name;
        public string? Name
        {
            get => _Name;
            set
            {
                SetProperty(ref _Name, value);
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private OperationalStatus _OperationalStatus;
        public OperationalStatus OperationalStatus
        {
            get => _OperationalStatus;
            set => SetProperty(ref _OperationalStatus, value);
        }

        public InfoPageViewModel? ParentVM { get; internal set; }

        private string? _PhysicalAddress;
        public string? PhysicalAddress
        {
            get => _PhysicalAddress;
            set => SetProperty(ref _PhysicalAddress, value);
        }

        private NetworkInterfaceStatistics? _Statistics;
        public NetworkInterfaceStatistics? Statistics
        {
            get => _Statistics;
            set => SetProperty(ref _Statistics, value);
        }

        private string? _Speed;
        public string? Speed
        {
            get => _Speed;
            set => SetProperty(ref _Speed, value);
        }

        public NetworkInterfaceViewModel(NetworkInterface networkInterface)
        {
            Icon = networkInterface.NetworkInterfaceType switch
            {
                NetworkInterfaceType.Ethernet => "network",
                NetworkInterfaceType.Wireless80211 => "wifi",
                _ => "questionmark.diamond"
            };

            IPAddresses = string.Join("\n", networkInterface.GetIPProperties().UnicastAddresses.Select(ua => ua.Address));

            IsUp = networkInterface.OperationalStatus == OperationalStatus.Up;

            Name = networkInterface.Name;

            OperationalStatus = networkInterface.OperationalStatus;

            PhysicalAddress = BitConverter.ToString(networkInterface.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":");

            Speed = networkInterface.Speed switch
            {
                >= 1_000_000_000 => $"{networkInterface.Speed / 1_000_000_000} Tbits/s",
                >= 1_000_000 => $"{networkInterface.Speed / 1_000_000} Gbits/s",
                >= 1_000 => $"{networkInterface.Speed / 1_000} Mbits/s",
                >= 1 => $"{networkInterface.Speed} kbits/s",
                _ => networkInterface.Speed.ToString()
            };

            Statistics = new NetworkInterfaceStatistics();

            NetworkInterfacesService = DependencyService.Get<INetworkInterfacesService>();

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(token =>
            {
                if (ParentVM?.SelectedNetworkInterface == this)
                {
                    if (Statistics.TryUpdate(NetworkInterfacesService, this))
                        Device.BeginInvokeOnMainThread(() => OnPropertyChanged(nameof(Statistics)));
                }
            });
        }
    }
}
