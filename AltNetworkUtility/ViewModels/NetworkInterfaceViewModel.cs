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
        readonly INetworkInterfacesService NetworkInterfacesService;
        readonly Repositories.NetworkInterfaceRepository.Repository NetworkInterfaceRepository;

        public string BsdName { get; }

        public string DisplayName
        {
            get
            {
                if (LocalizedDisplayName != null && BsdName != null)
                    return $"{LocalizedDisplayName} ({BsdName})";

                if (LocalizedDisplayName != null)
                    return LocalizedDisplayName;

                if (BsdName != null)
                    return BsdName;

                return "(unknown)";
            }
        }

        public IconSpec? Icon
        {
            get
            {
                if (!NetworkInterfaceType.HasValue)
                    return null;

                string iconName = NetworkInterfaceType.Value switch
                {
                    System.Net.NetworkInformation.NetworkInterfaceType.Ethernet => "network",
                    System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 => "wifi",
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

        private bool? _IsUp = false;
        public bool? IsUp
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
            Color = IsUp.GetValueOrDefault() ? Color.FromRgb(0x34, 0xC8, 0x4A) : Color.FromRgb(0xFA, 0x4B, 0x49),
            Size = new Size(12, 12)
        };
        public string IsUpDescription => IsUp.GetValueOrDefault() ? "Connected" : "Not Connected";

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

        private string? _Model;
        public string? Model
        {
            get => _Model;
            set => SetProperty(ref _Model, value);
        }

        private NetworkInterfaceType? _NetworkInterfaceType;
        public NetworkInterfaceType? NetworkInterfaceType
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

        private string? _Vendor;
        public string? Vendor
        {
            get => _Vendor;
            set => SetProperty(ref _Vendor, value);
        }

        public NetworkInterfaceViewModel(string bsdName)
        {
            BsdName = bsdName;

            Statistics = new NetworkInterfaceStatistics();

            NetworkInterfaceRepository = DependencyService.Get<Repositories.NetworkInterfaceRepository.Repository>();

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(token =>
            {
                if (ParentVM?.SelectedNetworkInterface == this)
                {
                    if (Statistics.TryUpdate(NetworkInterfaceRepository, this))
                        Device.BeginInvokeOnMainThread(() => OnPropertyChanged(nameof(Statistics)));
                }
            });

        }

        public NetworkInterfaceViewModel(NetworkInterface networkInterface)
        {
            Log.Debug($"{networkInterface.Name}: " +
                      $"{networkInterface.NetworkInterfaceType}, {networkInterface.OperationalStatus}");

            BsdName = networkInterface.Name;

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
