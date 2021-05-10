using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

using AltNetworkUtility.macOS.Models;
using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using Foundation;

using ObjCRuntime;

#nullable enable

namespace AltNetworkUtility.macOS.Services
{
    public class MacNetworkInterfacesService : INetworkInterfacesService
    {
        private class NativeMethods
        {
            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceCopyAll();
        }

        public IEnumerable<NetworkInterfaceViewModel> GetAvailableInterfaces()
        {
            // CHECK: are BSD names always unique?

            var monoInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            var scNetworkInterfaces = new Dictionary<string, SCNetworkInterface>();

            using (var nativeInterfaces = Runtime.GetNSObject<NSArray>(NativeMethods.SCNetworkInterfaceCopyAll()))
            {
                for (nuint i = 0; i < nativeInterfaces.Count; i++)
                {
                    var item = new SCNetworkInterface(nativeInterfaces.ValueAt(i));
                    scNetworkInterfaces[item.BsdName] = item;

                }
            }

            var viewModels = new List<NetworkInterfaceViewModel>();

            foreach (var item in monoInterfaces)
            {
                var viewModel = new NetworkInterfaceViewModel(item);
                viewModels.Add(viewModel);

                if (scNetworkInterfaces.TryGetValue(item.Name, out var scNetworkInterface))
                {
                    viewModel.LocalizedDisplayName = scNetworkInterface.LocalizedDisplayName;
                }
            }

            return viewModels;
        }
    }
}
