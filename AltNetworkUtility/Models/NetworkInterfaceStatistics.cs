using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Models
{
    public class NetworkInterfaceStatistics
    {
        public struct RawValues
        {
            public ulong SentPackets;
            public ulong SendErrors;
            public ulong RecvPackets;
            public ulong RecvErrors;
            public ulong Collisions;
        }

        public RecentValues<ulong> SentPackets { get; set; } = new RecentValues<ulong>();
        public RecentValues<ulong> SendErrors { get; set; } = new RecentValues<ulong>();
        public RecentValues<ulong> RecvPackets { get; set; } = new RecentValues<ulong>();
        public RecentValues<ulong> RecvErrors { get; set; } = new RecentValues<ulong>();
        public RecentValues<ulong> Collisions { get; set; } = new RecentValues<ulong>();

        public bool TryUpdate(INetworkInterfacesService networkInterfacesService, NetworkInterfaceViewModel networkInterfaceViewModel)
        {
            if (!networkInterfacesService.TryGetStatistics(networkInterfaceViewModel, out var newValues))
                return false;

            SentPackets.EnqueueValue(newValues.Value.SentPackets);
            SendErrors.EnqueueValue(newValues.Value.SendErrors);
            RecvPackets.EnqueueValue(newValues.Value.RecvPackets);
            RecvErrors.EnqueueValue(newValues.Value.RecvErrors);
            Collisions.EnqueueValue(newValues.Value.Collisions);

            return true;
        }
    }
}
