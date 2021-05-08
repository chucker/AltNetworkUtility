using System.Collections.ObjectModel;
using System.Threading.Tasks;

using AltNetworkUtility.Services;

using Xamarin.Forms;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Tabs
{
    public class InfoPageViewModel : ViewModelBase
    {
        // TODO: should probably not be a string later (do we want to use NetworkInterface?)
        public ObservableCollection<NetworkInterfaceViewModel> AvailableNetworkInterfaces { get; } = new();

        public async Task InitAsync()
        {
            var svc = DependencyService.Get<INetworkInterfacesService>();

            foreach (var item in svc.GetAvailableInterfaces())
            {
                AvailableNetworkInterfaces.Add(item);
            }            
        }
    }
}
