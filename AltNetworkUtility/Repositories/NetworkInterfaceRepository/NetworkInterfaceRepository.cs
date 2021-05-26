using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Repositories.NetworkInterfaceRepository
{
    public class NetworkInterfaceRepository
    {
        private readonly Dictionary<DataSourceKind, IDataSource> _DataSources = new();
        public void RegisterDataSource(IDataSource dataSource)
            => _DataSources[dataSource.Kind] = dataSource;

        public ObservableCollection<NetworkInterfaceViewModel> AsObservable { get; } = new();

        public void ReloadAll(DataSourceKind dataSourceKinds = DataSourceKind.All)
        {
            // TODO precedence

            AsObservable.Clear();

            var bsdNames = new List<string>();

            foreach (var dataSource in _DataSources.Values)
            {
                if (!dataSourceKinds.HasFlag(dataSource.Kind))
                    continue;

                var results = dataSource.GetAll();

                foreach (var newItem in results)
                {
                    if (!bsdNames.Contains(newItem.BsdName))
                    {
                        AsObservable.Add(newItem);
                        bsdNames.Add(newItem.BsdName);
                    }
                    else
                    {
                        /*
                         * UGLY: this is a very primitive approach to prioritization.
                         * It effectively relies on the order of registration being
                         * good.
                         */

                        var oldItem = AsObservable.Single(x => x.BsdName == newItem.BsdName);

                        if (oldItem.IPAddresses==null)
                            oldItem.IPAddresses = newItem.IPAddresses;

                        if (oldItem.IsUp==null)
                            oldItem.IsUp = newItem.IsUp;

                        if (oldItem.NetworkInterfaceType == null)
                            oldItem.NetworkInterfaceType = newItem.NetworkInterfaceType;

                        if (oldItem.PhysicalAddress == null)
                            oldItem.PhysicalAddress = newItem.PhysicalAddress;

                        if (oldItem.Speed == null)
                            oldItem.Speed = newItem.Speed;

                        // merge info somehow
                        //var existAsObservable.First(ni => ni.Name == item.Name).
                    }
                }
            }
        }
    }
}
