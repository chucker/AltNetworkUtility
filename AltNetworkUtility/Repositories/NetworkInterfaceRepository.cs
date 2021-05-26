using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Repositories
{
    public class NetworkInterfaceRepository
    {
        /// <summary>
        /// We need multiple different sources to get a comprehensive, accurate
        /// view of which network interfaces exist.
        ///
        /// In the nicest case, Mono/.NET (System.Net.NetworkInformation)
        /// provides this info. But it's incomplete and often wrong.
        ///
        /// BSD likewise is also incorrect in some places.
        ///
        /// Apple's IORegistry and SystemConfiguration APIs tend to be the most
        /// accurate.
        /// </summary>
        [Flags]
        public enum DataSourceKind
        {
            Bsd = 1 << 0,
            IoRegistry = 1 << 1,
            Mono = 1 << 2,
            SystemConfiguration = 1 << 3,

            All = Bsd | IoRegistry | Mono | SystemConfiguration
        }

        public interface IDataSource
        {
            DataSourceKind Kind { get; }

            NetworkInterfaceViewModel[] GetAll();
        }

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

                foreach (var nivm in results)
                {
                    if (!bsdNames.Contains(nivm.BsdName!))
                    {
                        AsObservable.Add(nivm);
                        bsdNames.Add(nivm.BsdName!);
                    }
                    else
                    {
                        // merge info somehow
                        //var existAsObservable.First(ni => ni.Name == item.Name).
                    }
                }
            }
        }

        public class MonoDataSource : IDataSource
        {
            public DataSourceKind Kind => DataSourceKind.Mono;

            public NetworkInterfaceViewModel[] GetAll()
            {
                return System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    .Select(ni => new NetworkInterfaceViewModel(ni.Name)
                    {
                        
                    }).ToArray();
            }
        }
    }
}
