using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.macOS.Services
{
    public class MacNetworkInterfacesService : INetworkInterfacesService
    {
        public IEnumerable<NetworkInterfaceViewModel> GetAvailableInterfaces()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            return interfaces.Select(ni => new NetworkInterfaceViewModel(ni));
        }
    }
}
