using System.Collections.ObjectModel;
using System.Threading.Tasks;

using FontAwesome5.Extensions;

using AltNetworkUtility.Services;

using Xamarin.Forms;
using FontAwesome5;
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

        public ImageSource FAIcon { get; set; }

        public InfoPageViewModel()
        {
            //if (FontAwesome5.EFontAwesomeIcon.Solid_Ethernet.GetSvg(out var path, out var width, out var height))
            //    SvgPath = path;

            FAIcon = CreateImageSource( EFontAwesomeIcon.Brands_Accusoft);
        }

        public static ImageSource CreateImageSource(EFontAwesomeIcon icon)
        {
            return Xamarin.Forms.DependencyService.Get<IIconFontProvider>().GetImage("car.fill");
        }
    }
}
