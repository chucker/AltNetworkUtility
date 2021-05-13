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

        public IEnumerable<NetworkInterfaceViewModel> GetAvailableInterfaces()
        {
            // CHECK: are BSD names always unique?

            var monoNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            var scNetworkInterfaces = GetSystemConfigurationNetworkInterfaces();

            var bsdNetworkInterfaceStatistics = GetBsdNetworkInterfaceStatistics();

            var viewModels = new List<NetworkInterfaceViewModel>();

            foreach (var item in monoNetworkInterfaces)
            {
                var viewModel = new NetworkInterfaceViewModel(item);
                viewModels.Add(viewModel);

                if (scNetworkInterfaces.TryGetValue(item.Name, out var scNetworkInterface))
                {
                    viewModel.LocalizedDisplayName = scNetworkInterface.LocalizedDisplayName;
                }

                if (bsdNetworkInterfaceStatistics.TryGetValue(item.Name, out var statistics))
                {
                    viewModel.SentPackets = statistics.ifi_opackets;
                    viewModel.SendErrors = statistics.ifi_oerrors;
                    viewModel.RecvPackets = statistics.ifi_ipackets;
                    viewModel.RecvErrors = statistics.ifi_ierrors;
                    viewModel.Collisions = statistics.ifi_collisions;
                }
            }

            return viewModels;
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

        private static Dictionary<string, NativeMethods.if_data64> GetBsdNetworkInterfaceStatistics()
        {
            var dict = new Dictionary<string, NativeMethods.if_data64>();

            if (NativeMethods.getifaddrs(out var ifPointer) != 0)
                return dict;

            while (ifPointer != IntPtr.Zero)
            {
                var addr = Marshal.PtrToStructure<NativeMethods.ifaddrs>(ifPointer);

                var sockaddr = Marshal.PtrToStructure<NativeMethods.sockaddr>(addr.ifa_addr);

                if ((NativeMethods.sockaddr_family)sockaddr.sa_family == NativeMethods.sockaddr_family.AF_LINK)
                {
                    var data = Marshal.PtrToStructure<NativeMethods.if_data64>(addr.ifa_data);

                    dict[addr.ifa_name] = data;
                }

                ifPointer = addr.ifa_next;
            }

            NativeMethods.freeifaddrs(ifPointer);

            return dict;
        }
    }
}
