using System;
using System.Linq;

using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Repositories.NetworkInterfaceRepository
{
    public class MonoDataSource : IDataSource
    {
        public DataSourceKind Kind => DataSourceKind.Mono;

        public NetworkInterfaceViewModel[] GetAll()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Select(ni => new NetworkInterfaceViewModel(ni.Name)
                {
                    IPAddresses = ni.GetIPProperties().UnicastAddresses
                                    .Select(ua => ua.Address)
                                    .ToArray(),
                    NetworkInterfaceType = ni.NetworkInterfaceType,
                    PhysicalAddress = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":"),
                    Speed = ni.Speed switch
                    {
                        >= 1_000_000_000 => $"{ni.Speed / 1_000_000_000} Tbits/s",
                        >= 1_000_000 => $"{ni.Speed / 1_000_000} Gbits/s",
                        >= 1_000 => $"{ni.Speed / 1_000} Mbits/s",
                        >= 1 => $"{ni.Speed} kbits/s",
                        _ => ni.Speed.ToString()
                    }
                }).ToArray();
        }
    }
}
