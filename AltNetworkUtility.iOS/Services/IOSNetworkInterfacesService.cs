using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

using AltNetworkUtility.Models;
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

        public bool TryFindInterfaceByName(string specificInterface, out NetworkInterfaceViewModel netIf)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetStatistics(NetworkInterfaceViewModel viewModel, out NetworkInterfaceStatistics.RawValues? statistics)
        {
            statistics = null;
            return false;
        }
    }
}
