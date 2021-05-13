using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs
{
    public class InfoPageViewModel : ViewModelBase
    {
        public ObservableCollection<NetworkInterfaceViewModel> AvailableNetworkInterfaces { get; } = new();

        private NetworkInterfaceViewModel? _SelectedNetworkInterface;
        public NetworkInterfaceViewModel? SelectedNetworkInterface { get => _SelectedNetworkInterface; set => SetProperty(ref _SelectedNetworkInterface, value); }

        public void Init()
        {
            var svc = DependencyService.Get<INetworkInterfacesService>();

            foreach (var item in svc.GetAvailableInterfaces())
            {
                AvailableNetworkInterfaces.Add(item);
            }
        }
    }
}
