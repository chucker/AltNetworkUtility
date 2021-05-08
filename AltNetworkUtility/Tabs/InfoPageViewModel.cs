using System.Collections.ObjectModel;
using System.Threading.Tasks;

using AltNetworkUtility.Services;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs
{
    public class InfoPageViewModel : ViewModelBase
    {
        // TODO: should probably not be a string later (do we want to use NetworkInterface?)
        public ObservableCollection<string> AvailableNetworkInterfaces { get; } = new();

        public async Task InitAsync()
        {
            var svc = DependencyService.Get<INetworkInterfacesService>();

            foreach (var item in await svc.GetAvailableInterfacesAsync())
            {
                AvailableNetworkInterfaces.Add(item);
            }            
        }
    }
}
