using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using AltNetworkUtility.Services;

namespace AltNetworkUtility.macOS.Services
{
    public class MacNetworkInterfacesService : INetworkInterfacesService
    {
        public async Task<IEnumerable<string>> GetAvailableInterfacesAsync()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            return interfaces.Select(i => i.Name);
        }
    }
}
