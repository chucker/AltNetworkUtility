﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

using AltNetworkUtility.macOS.Models;
using AltNetworkUtility.Models;
using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using Foundation;

using ObjCRuntime;

#nullable enable

namespace AltNetworkUtility.macOS.Services
{
    public class MacNetworkInterfacesService : INetworkInterfacesService
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<MacNetworkInterfacesService>();

#pragma warning disable CS0649
        private class NativeMethods
        {
            [Obsolete(nameof(AltNetworkUtility.Repositories.NetworkInterfaceRepository))]
            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceCopyAll();

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

            // see https://stackoverflow.com/a/8014012/1600
            // and https://gist.github.com/brendanzagaeski/9979929

            [DllImport("libc")]
            public static extern int getifaddrs(out IntPtr ifap);

            [DllImport("libc")]
            public static extern void freeifaddrs(IntPtr ifap);

            public struct ifaddrs
            {
                public IntPtr ifa_next;
                public string ifa_name;
                public uint ifa_flags;
                public IntPtr ifa_addr;
                public IntPtr ifa_netmask;
                public IntPtr ifa_dstaddr;
                public IntPtr ifa_data;
            }

            public struct sockaddr
            {
                public byte sa_len;
                public byte sa_family;
            }

            public enum sockaddr_family : byte
            {
                AF_INET = 2,
                AF_INET6 = 30,
                AF_LINK = 18
            }

            public struct if_data
            {
                /* generic interface information */
                public byte ifi_type;          /* ethernet, tokenring, etc */
                public byte ifi_typelen;       /* Length of frame type id */
                public byte ifi_physical;      /* e.g., AUI, Thinnet, 10base-T, etc */
                public byte ifi_addrlen;       /* media address length */
                public byte ifi_hdrlen;        /* media header length */
                public byte ifi_recvquota;     /* polling quota for receive intrs */
                public byte ifi_xmitquota;     /* polling quota for xmit intrs */
                public byte ifi_unused1;       /* for future use */
                public uint ifi_mtu;           /* maximum transmission unit */
                public uint ifi_metric;        /* routing metric (external only) */
                public uint ifi_baudrate;      /* linespeed */
                /* volatile statistics */
                public uint ifi_ipackets;      /* packets received on interface */
                public uint ifi_ierrors;       /* input errors on interface */
                public uint ifi_opackets;      /* packets sent on interface */
                public uint ifi_oerrors;       /* output errors on interface */
                public uint ifi_collisions;    /* collisions on csma interfaces */
                public uint ifi_ibytes;        /* total number of octets received */
                public uint ifi_obytes;        /* total number of octets sent */
                public uint ifi_imcasts;       /* packets received via multicast */
                public uint ifi_omcasts;       /* packets sent via multicast */
                public uint ifi_iqdrops;       /* dropped on input, this interface */
                public uint ifi_noproto;       /* destined for unsupported protocol */
                public uint ifi_recvtiming;    /* usec spent receiving when timing */
                public uint ifi_xmittiming;    /* usec spent xmitting when timing */
                public IntPtr ifi_lastchange;  /* time of last administrative change */
                public uint ifi_unused2;       /* used to be the default_proto */
                public uint ifi_hwassist;      /* HW offload capabilities */
                public uint ifi_reserved1;     /* for future use */
                public uint ifi_reserved2;     /* for future use */
            };

            /*
             * we can't actually use this (at this point), because we would need
             * an equivalent of getifaddrs for it.
             */
            public struct if_data64
            {
                /* generic interface information */
                public byte ifi_type;                /* ethernet, tokenring, etc */
                public byte ifi_typelen;             /* Length of frame type id */
                public byte ifi_physical;            /* e.g., AUI, Thinnet, 10base-T, etc */
                public byte ifi_addrlen;             /* media address length */
                public byte ifi_hdrlen;              /* media header length */
                public byte ifi_recvquota;           /* polling quota for receive intrs */
                public byte ifi_xmitquota;           /* polling quota for xmit intrs */
                public byte ifi_unused1;             /* for future use */
                public uint ifi_mtu;                 /* maximum transmission unit */
                public uint ifi_metric;              /* routing metric (external only) */
                public ulong ifi_baudrate;           /* linespeed */
                /* volatile statistics */
                public ulong ifi_ipackets;           /* packets received on interface */
                public ulong ifi_ierrors;            /* input errors on interface */
                public ulong ifi_opackets;           /* packets sent on interface */
                public ulong ifi_oerrors;            /* output errors on interface */
                public ulong ifi_collisions;         /* collisions on csma interfaces */
                public ulong ifi_ibytes;             /* total number of octets received */
                public ulong ifi_obytes;             /* total number of octets sent */
                public ulong ifi_imcasts;            /* packets received via multicast */
                public ulong ifi_omcasts;            /* packets sent via multicast */
                public ulong ifi_iqdrops;            /* dropped on input, this interface */
                public ulong ifi_noproto;            /* destined for unsupported protocol */
                public uint ifi_recvtiming;          /* usec spent receiving when timing */
                public uint ifi_xmittiming;          /* usec spent xmitting when timing */
                public IntPtr ifi_lastchange;        /* time of last administrative change */
            };
        }
#pragma warning restore CS0649

        private List<NetworkInterfaceViewModel>? _Interfaces;

        public IEnumerable<NetworkInterfaceViewModel> GetAvailableInterfaces()
        {
            // CHECK: are BSD names always unique?

            var monoNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            var scNetworkInterfaces = GetSystemConfigurationNetworkInterfaces();

            var iokitNetworkInterfaces = GetIORegistryNetworkInterfaces();

            var viewModels = new List<NetworkInterfaceViewModel>();

            foreach (var item in monoNetworkInterfaces)
            {
                var viewModel = new NetworkInterfaceViewModel(item);
                viewModels.Add(viewModel);

                if (scNetworkInterfaces.TryGetValue(item.Name, out var scNetworkInterface))
                {
                    viewModel.LocalizedDisplayName = scNetworkInterface.LocalizedDisplayName;

                    // this may be obsolete once .NET 6 rolls out:
                    // https://github.com/dotnet/runtime/pull/43737
                    viewModel.NetworkInterfaceType = (string)scNetworkInterface.InterfaceType switch
                    {
                        "IEEE80211" => NetworkInterfaceType.Wireless80211,
                        "Ethernet" => NetworkInterfaceType.Ethernet,
                        _ => NetworkInterfaceType.Unknown
                    };

                    if (viewModel.NetworkInterfaceType == NetworkInterfaceType.Unknown)
                        Log.Information($"Unknown network interface type for {item.Name}. " +
                                        $"macOS gives {nameof(scNetworkInterface.LocalizedDisplayName)} {scNetworkInterface.LocalizedDisplayName}, " +
                                        $"{nameof(scNetworkInterface.InterfaceType)} {scNetworkInterface.InterfaceType}");

                    // Mono's IsUp cannot be trusted
                    viewModel.IsUp = scNetworkInterface.IsUp;
                }

                if(iokitNetworkInterfaces.TryGetValue(item.Name, out var ioKitNetworkInterface))
                {
                    viewModel.Vendor = ioKitNetworkInterface.Vendor;
                    viewModel.Model = ioKitNetworkInterface.Model;
                }
            }

            _Interfaces = viewModels;

            return viewModels;
        }

        private Dictionary<string, (string Vendor, string Model)> GetIORegistryNetworkInterfaces()
        {
            var dict = new Dictionary<string, (string, string)>();

            var serviceDescription = NativeMethods.IOServiceMatching("IOEthernetInterface");
            int result = NativeMethods.IOServiceGetMatchingServices(0, serviceDescription, out var ioIterator);

            if (result != 0)
            {
                Log.Warning($"{nameof(NativeMethods.IOServiceGetMatchingServices)} returned {result}");
                return dict;
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

                dict[bsdName] = (ioVendor, ioModel);
            }

            //TODO? https://medium.com/@donblas/lets-bind-an-iokit-method-by-hand-fba939b54222
            //NativeMethods.IOServiceClose(ioIterator);

            return dict;
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

        private static Dictionary<string, SCNetworkInterface> GetSystemConfigurationNetworkInterfaces()
        {
            var scNetworkInterfaces = new Dictionary<string, SCNetworkInterface>();

            using (var nativeInterfaces = Runtime.GetNSObject<NSArray>(NativeMethods.SCNetworkInterfaceCopyAll()))
            {
                for (nuint i = 0; i < nativeInterfaces.Count; i++)
                {
                    var item = new SCNetworkInterface(nativeInterfaces.ValueAt(i));
                    scNetworkInterfaces[item.BsdName] = item;

                }
            }

            return scNetworkInterfaces;
        }

        public bool TryGetStatistics(NetworkInterfaceViewModel viewModel,
                                     [NotNullWhen(true)] out NetworkInterfaceStatistics.RawValues? statistics)
        {
            statistics = null;

            if (NativeMethods.getifaddrs(out var initialIfPointer) != 0)
                return false;

            try
            {
                IntPtr ifPointer = initialIfPointer;

                while (ifPointer != IntPtr.Zero)
                {
                    var addr = Marshal.PtrToStructure<NativeMethods.ifaddrs>(ifPointer);

                    var sockaddr = Marshal.PtrToStructure<NativeMethods.sockaddr>(addr.ifa_addr);

                    if ((NativeMethods.sockaddr_family)sockaddr.sa_family == NativeMethods.sockaddr_family.AF_LINK &&
                        addr.ifa_name == viewModel.BsdName)
                    {
                        var data = Marshal.PtrToStructure<NativeMethods.if_data>(addr.ifa_data);

                        statistics = new NetworkInterfaceStatistics.RawValues(data.ifi_opackets, data.ifi_obytes,
                                                                              data.ifi_oerrors, data.ifi_ipackets,
                                                                              data.ifi_ibytes, data.ifi_ierrors,
                                                                              data.ifi_collisions);

                        return true;
                    }

                    ifPointer = addr.ifa_next;
                }
            }
            finally
            {
                NativeMethods.freeifaddrs(initialIfPointer);
            }

            return false;
        }

        public bool TryFindInterfaceByName(string name,
                                           [NotNullWhen(true)] out NetworkInterfaceViewModel? networkInterfaceViewModel)
        {
            if (_Interfaces == null)
                throw new InvalidOperationException($"Call {nameof(GetAvailableInterfaces)} first");

            networkInterfaceViewModel = _Interfaces.Find(netIf => netIf.BsdName == name);

            return networkInterfaceViewModel != null;
        }
    }
}
