using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.iOS.Services
{
    public class IOSNetworkInterfacesService : INetworkInterfacesService
    {
        public IEnumerable<NetworkInterfaceViewModel> GetAvailableInterfaces()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            return interfaces.Select(ni => new NetworkInterfaceViewModel(ni));
        }
    }
}
