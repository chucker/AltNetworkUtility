using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using AltNetworkUtility.Models;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Repositories.NetworkInterfaceRepository
{
    public class Repository
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<Repository>();

        private readonly Dictionary<DataSourceKind, IDataSource> _DataSources = new();
        private IProvideStatistics? _StatisticsDataSource;
        public void RegisterDataSource(IDataSource dataSource)
        {
            _DataSources[dataSource.Kind] = dataSource;

            if (dataSource is IProvideStatistics statsProvider)
                _StatisticsDataSource = statsProvider;
        }

        public ObservableCollection<NetworkInterfaceViewModel> AsObservable { get; } = new();

        public void ReloadAll(DataSourceKind dataSourceKinds = DataSourceKind.All)
        {
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

                        if (oldItem.IPAddresses == null)
                            oldItem.IPAddresses = newItem.IPAddresses;

                        if (oldItem.IsUp == null)
                            oldItem.IsUp = newItem.IsUp;

                        if (oldItem.Model == null)
                            oldItem.Model = newItem.Model;

                        if (oldItem.NetworkInterfaceType == null)
                            oldItem.NetworkInterfaceType = newItem.NetworkInterfaceType;

                        if (oldItem.PhysicalAddress == null)
                            oldItem.PhysicalAddress = newItem.PhysicalAddress;

                        if (oldItem.Speed == null)
                            oldItem.Speed = newItem.Speed;

                        if (oldItem.Vendor == null)
                            oldItem.Vendor = newItem.Vendor;
                    }
                }
            }
        }

        public bool TryFindByBsdName(string bsdName,
                                     [NotNullWhen(true)] out NetworkInterfaceViewModel? result)
        {
            result = AsObservable.FirstOrDefault(ni => ni.BsdName == bsdName);

            return result != null;
        }

        public bool TryGetStatistics(NetworkInterfaceViewModel viewModel,
                                     [NotNullWhen(true)] out NetworkInterfaceStatistics.RawValues? statistics)
        {
            statistics = null;

            if (_StatisticsDataSource == null)
            {
                Log.Warning("Nothing provides network statistics right now");
                return false;
            }

            return _StatisticsDataSource.TryGet(viewModel, out statistics);

        }
    }
}
