using AltNetworkUtility.Repositories.NetworkInterfaceRepository;
using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using Microsoft.Toolkit.Mvvm.Input;

using Rotorsoft.Forms;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Input;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs.Info
{
    public class InfoPageViewModel : ViewModelBase
    {
        public ObservableCollection<NetworkInterfaceViewModel> AvailableNetworkInterfaces { get; }

        private Predicate<object> _NetworkInterfaceFilter;
        public Predicate<object> NetworkInterfaceFilter => _NetworkInterfaceFilter;

        public ICollectionView? NetworkInterfacesView { get; private set; }

        public PreferencesService Preferences { get; }

        private NetworkInterfaceViewModel? _SelectedNetworkInterface;

        public NetworkInterfaceViewModel? SelectedNetworkInterface
        {
            get => _SelectedNetworkInterface;
            set => SetProperty(ref _SelectedNetworkInterface, value);
        }

        public ICommand ToggleShowAllNetworkInterfacesCommand { get; }

        public InfoPageViewModel()
        {
            Preferences = PreferencesService.GetInstance<InfoPageViewModel>();

            _NetworkInterfaceFilter = o =>
            {
                if (o is not NetworkInterfaceViewModel networkInterface)
                    return false;

                bool shouldShowType = networkInterface.NetworkInterfaceType switch
                {
                    NetworkInterfaceType.AsymmetricDsl => true,
                    NetworkInterfaceType.Atm => true,
                    NetworkInterfaceType.BasicIsdn => true,
                    NetworkInterfaceType.Ethernet => true,
                    NetworkInterfaceType.Ethernet3Megabit => true,
                    NetworkInterfaceType.FastEthernetFx => true,
                    NetworkInterfaceType.FastEthernetT => true,
                    NetworkInterfaceType.Fddi => true,
                    NetworkInterfaceType.GenericModem => true,
                    NetworkInterfaceType.GigabitEthernet => true,
                    NetworkInterfaceType.HighPerformanceSerialBus => true,
                    NetworkInterfaceType.IPOverAtm => true,
                    NetworkInterfaceType.Isdn => true,
                    NetworkInterfaceType.MultiRateSymmetricDsl => true,
                    NetworkInterfaceType.Ppp => true,
                    NetworkInterfaceType.PrimaryIsdn => true,
                    NetworkInterfaceType.RateAdaptDsl => true,
                    NetworkInterfaceType.Slip => true,
                    NetworkInterfaceType.SymmetricDsl => true,
                    NetworkInterfaceType.TokenRing => true,
                    NetworkInterfaceType.VeryHighSpeedDsl => true,
                    NetworkInterfaceType.Wireless80211 => true,
                    NetworkInterfaceType.Wman => true,
                    NetworkInterfaceType.Wwanpp => true,
                    NetworkInterfaceType.Wwanpp2 => true,

                    NetworkInterfaceType.Loopback => false,
                    NetworkInterfaceType.Tunnel => false,
                    _ => false
                };

                // Mono considers these "Ethernet"
                bool filterName = networkInterface.BsdName!.StartsWith("p2p") ||
                                  networkInterface.BsdName.StartsWith("awdl") ||
                                  networkInterface.BsdName.StartsWith("llw");

                return shouldShowType && !filterName;
            };

            var repo = DependencyService.Get<Repository>();
            AvailableNetworkInterfaces = repo.AsObservable;

            // workaround for #36 interface picker, where filtering doesn't appear to work
            foreach (var item in AvailableNetworkInterfaces.ToList())
            {
                if (!NetworkInterfaceFilter(item))
                    AvailableNetworkInterfaces.Remove(item);
            }

            SelectedNetworkInterface = AvailableNetworkInterfaces.FirstOrDefault();
        }

        public void Init()
        {
            foreach (var item in AvailableNetworkInterfaces)
                item.ParentVM = this;
        }
    }
}
