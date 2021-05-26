using System;

namespace AltNetworkUtility.Repositories.NetworkInterfaceRepository
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
}
