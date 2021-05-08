using System.Collections.Generic;
using System.Threading.Tasks;

namespace AltNetworkUtility.Services
{
    public interface INetworkInterfacesService
    {
        Task<IEnumerable<string>> GetAvailableInterfacesAsync();
    }
}
