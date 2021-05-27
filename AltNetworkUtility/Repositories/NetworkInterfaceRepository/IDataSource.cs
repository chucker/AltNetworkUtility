using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Repositories.NetworkInterfaceRepository
{
    public interface IDataSource
    {
        DataSourceKind Kind { get; }

        NetworkInterfaceViewModel[] GetAll();
    }
}
