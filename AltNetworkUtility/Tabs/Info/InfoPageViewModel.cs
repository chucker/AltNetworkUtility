using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using Microsoft.Toolkit.Mvvm.Input;

using Rotorsoft.Forms;

using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows.Input;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs.Info
{
    public class InfoPageViewModel : ViewModelBase
    {
        public ObservableCollection<NetworkInterfaceViewModel> AvailableNetworkInterfaces { get; } = new();

        private Predicate<object> _NetworkInterfaceFilter;
        public Predicate<object> NetworkInterfaceFilter => _NetworkInterfaceFilter;

        public ICollectionView? NetworkInterfacesView { get; private set; }

        public PreferencesService Preferences { get; }

        private NetworkInterfaceViewModel? _SelectedNetworkInterface;

        public NetworkInterfaceViewModel? SelectedNetworkInterface { get => _SelectedNetworkInterface; set => SetProperty(ref _SelectedNetworkInterface, value); }

        private bool _ShowAllNetworkInterfaces;
        public bool ShowAllNetworkInterfaces
        {
            get => _ShowAllNetworkInterfaces;
            set
            {
                SetProperty(ref _ShowAllNetworkInterfaces, value);

                Preferences.Set(nameof(ShowAllNetworkInterfaces), value);

                NetworkInterfacesView?.Refresh();
            }
        }

        public ICommand ToggleShowAllNetworkInterfacesCommand { get; }

        public InfoPageViewModel()
        {
            Preferences = PreferencesService.GetInstance<InfoPageViewModel>();

            ShowAllNetworkInterfaces = Preferences.Get(nameof(ShowAllNetworkInterfaces), false);

            _NetworkInterfaceFilter = o =>
            {
                if (o is not NetworkInterfaceViewModel networkInterface)
                    return false;

                if (ShowAllNetworkInterfaces)
                    return true;

                return networkInterface.ShouldCommonlyShow;
            };

            ToggleShowAllNetworkInterfacesCommand = new RelayCommand(() =>
                ShowAllNetworkInterfaces = !ShowAllNetworkInterfaces);
        }

        public void Init(ICollectionView? networkInterfacesView)
        {
            NetworkInterfacesView = networkInterfacesView;

            var svc = DependencyService.Get<INetworkInterfacesService>();

            var interfaces = svc.GetAvailableInterfaces();

            foreach (var item in interfaces)
            {
                AvailableNetworkInterfaces.Add(item);

                item.ParentVM = this;
            }
        }
    }
}
