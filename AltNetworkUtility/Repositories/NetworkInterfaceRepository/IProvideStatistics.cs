using System.Diagnostics.CodeAnalysis;

using AltNetworkUtility.Models;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Repositories.NetworkInterfaceRepository
{
    public interface IProvideStatistics
    {
        bool TryGet(NetworkInterfaceViewModel viewModel,
                    [NotNullWhen(true)] out NetworkInterfaceStatistics.RawValues? statistics);
    }
}
