using System.Collections.Generic;

using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Services
{
    public interface INetworkInterfacesService
    {
        IEnumerable<NetworkInterfaceViewModel> GetAvailableInterfaces();
    }
}
