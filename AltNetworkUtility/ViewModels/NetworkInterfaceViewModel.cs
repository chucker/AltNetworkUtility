using System;
using System.Net.NetworkInformation;
using System.Reactive.Linq;

namespace AltNetworkUtility.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
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

        private string? _PhysicalAddress;
        public string? PhysicalAddress
        {
            get => _PhysicalAddress;
            set => SetProperty(ref _PhysicalAddress, value);
        }

        private double _RandomNumber;
        public double RandomNumber
        {
            get => _RandomNumber;
            set => SetProperty(ref _RandomNumber, value);
        }

        private string? _Speed;
        public string? Speed
        {
            get => _Speed;
            set => SetProperty(ref _Speed, value);
        }

        private ulong _SentPackets;
        public ulong SentPackets
        {
            get => _SentPackets;
            set => SetProperty(ref _SentPackets, value);
        }

        private ulong _SendErrors;
        public ulong SendErrors
        {
            get => _SendErrors;
            set => SetProperty(ref _SendErrors, value);
        }

        private ulong _RecvPackets;
        public ulong RecvPackets
        {
            get => _RecvPackets;
            set => SetProperty(ref _RecvPackets, value);
        }

        private ulong _RecvErrors;
        public ulong RecvErrors
        {
            get => _RecvErrors;
            set => SetProperty(ref _RecvErrors, value);
        }

        private ulong _Collisions;
        public ulong Collisions
        {
            get => _Collisions;
            set => SetProperty(ref _Collisions, value);
        }

        public NetworkInterfaceViewModel(NetworkInterface networkInterface)
        {
            Icon = networkInterface.NetworkInterfaceType switch
            {
                NetworkInterfaceType.Ethernet => "network",
                NetworkInterfaceType.Wireless80211 => "wifi",
                _ => "questionmark.diamond"
            };

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

            // TODO: keep updating network statistics
            //var random = new Random();
            //Observable.Interval(TimeSpan.FromMilliseconds(10)).Subscribe(token => RandomNumber += random.NextDouble());
        }
    }
}
