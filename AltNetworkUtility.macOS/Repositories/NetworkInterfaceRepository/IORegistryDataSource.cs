using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using AltNetworkUtility.Repositories.NetworkInterfaceRepository;
using AltNetworkUtility.ViewModels;

using Foundation;

using ObjCRuntime;

#nullable enable

namespace AltNetworkUtility.macOS.Repositories.NetworkInterfaceRepository
{
    public class IORegistryDataSource : IDataSource
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<IORegistryDataSource>();

        private class NativeMethods
        {
            [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
            public static extern int IOServiceGetMatchingServices(int masterPort, IntPtr matching, out IntPtr existing);

            [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
            public static extern IntPtr IOServiceMatching(string name);

            [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
            public extern static int IOServiceClose(int service);

            [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
            public static extern IntPtr IOIteratorNext(IntPtr iterator);

            [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
            public static extern IntPtr IORegistryEntryCreateCFProperty(IntPtr entry, IntPtr key, IntPtr allocator, int options);

            [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
            public static extern int IORegistryEntryGetParentEntry(IntPtr entry, string plane, out IntPtr parent);
        }

        public DataSourceKind Kind => DataSourceKind.IORegistry;

        public NetworkInterfaceViewModel[] GetAll()
        {
            var results = new List<NetworkInterfaceViewModel>();

            var serviceDescription = NativeMethods.IOServiceMatching("IOEthernetInterface");
            int result = NativeMethods.IOServiceGetMatchingServices(0, serviceDescription, out var ioIterator);

            if (result != 0)
            {
                Log.Warning($"{nameof(NativeMethods.IOServiceGetMatchingServices)} returned {result}");
                return results.ToArray();
            }

            string? bsdName;

            IntPtr current;
            while ((current = NativeMethods.IOIteratorNext(ioIterator)) != IntPtr.Zero)
            {
                if (!TryLookupIOKitStringProperty("BSD Name", current, out bsdName))
                {
                    Log.Warning("Couldn't look up BSD name");
                    continue;
                }

                result = NativeMethods.IORegistryEntryGetParentEntry(current, "IOService", out var parent);

                if (result != 0)
                {
                    Log.Warning($"Couldn't look up parent for {bsdName}");
                    continue;
                }

                if (!TryLookupIOKitStringProperty("IOVendor", parent, out string? ioVendor))
                    continue;
                if (!TryLookupIOKitStringProperty("IOModel", parent, out string? ioModel))
                    continue;

                results.Add(new NetworkInterfaceViewModel(bsdName)
                {
                    Vendor = ioVendor,
                    Model = ioModel
                });
            }

            //TODO? https://medium.com/@donblas/lets-bind-an-iokit-method-by-hand-fba939b54222
            //NativeMethods.IOServiceClose(ioIterator);

            return results.ToArray();
        }

        private bool TryLookupIOKitStringProperty(string key, IntPtr ioRegistryEntry,
                                                  [NotNullWhen(true)] out string? stringValue)
        {
            stringValue = null;

            using (var _key = new NSString(key))
            {
                var prop = NativeMethods.IORegistryEntryCreateCFProperty(ioRegistryEntry, _key.Handle, IntPtr.Zero, 0);

                if (prop == IntPtr.Zero)
                {
                    Log.Warning("Couldn't look up BSD name");
                    return false;
                }

                stringValue = Runtime.GetNSObject<NSString>(prop);
                return true;
            }
        }
    }
}
