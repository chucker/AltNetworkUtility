using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using AltNetworkUtility.Models;
using AltNetworkUtility.Repositories.NetworkInterfaceRepository;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.macOS.Repositories.NetworkInterfaceRepository
{
    public class BsdDataSource : IDataSource, IProvideStatistics
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<BsdDataSource>();

        private class NativeMethods
        {
#pragma warning disable CS0649
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
#pragma warning restore CS0649
        }

        public DataSourceKind Kind => DataSourceKind.Bsd;

        public NetworkInterfaceViewModel[] GetAll()
        {
            return Array.Empty<NetworkInterfaceViewModel>();
        }

        public bool TryGet(NetworkInterfaceViewModel viewModel,
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
    }
}
