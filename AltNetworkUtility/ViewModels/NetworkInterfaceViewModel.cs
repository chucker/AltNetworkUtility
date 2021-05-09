using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace AltNetworkUtility.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
        private string? _Description;
        public string? Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
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

        private string? _Name;
        public string? Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        private string? _PhysicalAddress;
        public string? PhysicalAddress
        {
            get => _PhysicalAddress;
            set => SetProperty(ref _PhysicalAddress, value);
        }

        private string? _Speed;
        public string? Speed
        {
            get => _Speed;
            set => SetProperty(ref _Speed, value);
        }

        public NetworkInterfaceViewModel(NetworkInterface networkInterface)
        {
            Description = networkInterface.Description;

            Icon = networkInterface.NetworkInterfaceType switch
            {
                NetworkInterfaceType.Ethernet => "network",
                NetworkInterfaceType.Wireless80211 => "wifi",
                _ => "questionmark.diamond"
            };

            IPAddresses = string.Join("\t", networkInterface.GetIPProperties().UnicastAddresses.Select(ua => ua.Address));

            IsUp = networkInterface.OperationalStatus == OperationalStatus.Up;

            Name = networkInterface.Name;
            
            PhysicalAddress = BitConverter.ToString(networkInterface.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":");

            Speed = networkInterface.Speed switch
            {
                >= 1_000_000_000 => $"{networkInterface.Speed / 1_000_000_000} Tbits/s",
                >= 1_000_000 => $"{networkInterface.Speed / 1_000_000} Gbits/s",
                >= 1_000 => $"{networkInterface.Speed / 1_000} Mbits/s",
                >= 1 => $"{networkInterface.Speed} kbits/s",
                _ => networkInterface.Speed.ToString()
            };
        }
    }
}
