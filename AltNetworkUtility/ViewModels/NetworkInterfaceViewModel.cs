using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;

using AltNetworkUtility.Models;
using AltNetworkUtility.Services;
using AltNetworkUtility.Services.IconFont;
using AltNetworkUtility.Tabs.Info;

using Xamarin.Forms;

namespace AltNetworkUtility.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<NetworkInterfaceViewModel>();

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

        public IconSpec Icon
        {
            get
            {
                string iconName = NetworkInterfaceType switch
                {
                    NetworkInterfaceType.Ethernet => "network",
                    NetworkInterfaceType.Wireless80211 => "wifi",
                    _ => "questionmark.diamond"
                };

                return new IconSpec(iconName)
                {
                    Size = new Size(24, 24)
                };
            }
        }

        private IPAddress[]? _IPAddresses;
        public IPAddress[]? IPAddresses
        {
            get => _IPAddresses;
            set => SetProperty(ref _IPAddresses, value);
        }

        private bool _IsUp = false;
        public bool IsUp
        {
            get => _IsUp;
            set
            {
                SetProperty(ref _IsUp, value);

                OnPropertyChanged(nameof(IsUpDescription));
            }
        }

        public IconSpec IsUpImage => new("circle.fill")
        {
            Color = IsUp ? Color.FromRgb(0x34, 0xC8, 0x4A) : Color.FromRgb(0xFA, 0x4B, 0x49),
            Size = new Size(12, 12)
        };
        public string IsUpDescription => IsUp ? "Connected" : "Not Connected";

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

        private NetworkInterfaceType _NetworkInterfaceType;
        public NetworkInterfaceType NetworkInterfaceType
        {
            get => _NetworkInterfaceType;
            set
            {
                SetProperty(ref _NetworkInterfaceType, value);

                OnPropertyChanged(nameof(Icon));
            }
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
            Log.Debug($"{networkInterface.Name}: " +
                      $"{networkInterface.NetworkInterfaceType}, {networkInterface.OperationalStatus}");

            IPAddresses = networkInterface.GetIPProperties().UnicastAddresses.Select(ua => ua.Address).ToArray();

            Name = networkInterface.Name;

            NetworkInterfaceType = networkInterface.NetworkInterfaceType;

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
