using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using AltNetworkUtility.Models;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Services
{
    public interface INetworkInterfacesService
    {
        IEnumerable<NetworkInterfaceViewModel> GetAvailableInterfaces();

        bool TryGetStatistics(NetworkInterfaceViewModel viewModel,
                              [NotNullWhen(true)] out NetworkInterfaceStatistics.RawValues? statistics);
    }
}
