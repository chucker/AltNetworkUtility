using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

using AltNetworkUtility.macOS.Models;
using AltNetworkUtility.Repositories.NetworkInterfaceRepository;
using AltNetworkUtility.ViewModels;

using Foundation;

using ObjCRuntime;

namespace AltNetworkUtility.macOS.Repositories.NetworkInterfaceRepository
{
    public class SystemConfigurationDataSource : IDataSource
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<SystemConfigurationDataSource>();

        private class NativeMethods
        {
            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceCopyAll();
        }

        public DataSourceKind Kind => DataSourceKind.SystemConfiguration;

        public NetworkInterfaceViewModel[] GetAll()
        {
            var results = new List<NetworkInterfaceViewModel>();

            using (var nativeInterfaces = Runtime.GetNSObject<NSArray>(NativeMethods.SCNetworkInterfaceCopyAll()))
            {
                for (nuint i = 0; i < nativeInterfaces.Count; i++)
                {
                    var scni = new SCNetworkInterface(nativeInterfaces.ValueAt(i));

                    var nivm = new NetworkInterfaceViewModel(scni.BsdName)
                    {
                        LocalizedDisplayName = scni.LocalizedDisplayName,

                        NetworkInterfaceType = (string)scni.InterfaceType switch
                        {
                            "IEEE80211" => NetworkInterfaceType.Wireless80211,
                            "Ethernet" => NetworkInterfaceType.Ethernet,
                            _ => NetworkInterfaceType.Unknown
                        }
                    };

                    if (nivm.NetworkInterfaceType == NetworkInterfaceType.Unknown)
                        Log.Information($"Unknown network interface type for {scni.BsdName}. " +
                                        $"macOS gives {nameof(scni.LocalizedDisplayName)} {scni.LocalizedDisplayName}, " +
                                        $"{nameof(scni.InterfaceType)} {scni.InterfaceType}");

                    nivm.IsUp = scni.IsUp;

                    results.Add(nivm);
                }
            }

            return results.ToArray();
        }
    }
}

