using AltNetworkUtility.ViewModels;

namespace AltNetworkUtility.Models
{
    public class NetworkInterfaceStatistics
    {
        public readonly struct RawValues
        {
            public RawValues(ulong sentPackets, ulong sentBytes, ulong sendErrors, ulong recvPackets, ulong recvBytes, ulong recvErrors, ulong collisions)
            {
                SentPackets = sentPackets;
                SentBytes = sentBytes;
                SendErrors = sendErrors;
                RecvPackets = recvPackets;
                RecvBytes = recvBytes;
                RecvErrors = recvErrors;
                Collisions = collisions;
            }

            public ulong SentPackets { get; }
            public ulong SentBytes { get; }
            public ulong SendErrors { get; }
            public ulong RecvPackets { get; }
            public ulong RecvBytes { get; }
            public ulong RecvErrors { get; }
            public ulong Collisions { get; }
        }

        public RecentValues<ulong> SentPackets { get; set; } = new RecentValues<ulong> { RetainSeconds = 60 };
        public RecentValues<ulong> SentBytes { get; set; } = new RecentValues<ulong> { RetainSeconds = 60 };
        public RecentValues<ulong> SendErrors { get; set; } = new RecentValues<ulong> { RetainSeconds = 60 };
        public RecentValues<ulong> RecvPackets { get; set; } = new RecentValues<ulong> { RetainSeconds = 60 };
        public RecentValues<ulong> RecvBytes { get; set; } = new RecentValues<ulong> { RetainSeconds = 60 };
        public RecentValues<ulong> RecvErrors { get; set; } = new RecentValues<ulong> { RetainSeconds = 60 };
        public RecentValues<ulong> Collisions { get; set; } = new RecentValues<ulong> { RetainSeconds = 60 };

        public bool TryUpdate(Repositories.NetworkInterfaceRepository.Repository networkInterfaceRepository,
                              NetworkInterfaceViewModel networkInterfaceViewModel)
        {
            if (!networkInterfaceRepository.TryGetStatistics(networkInterfaceViewModel, out var newValues))
                return false;

            SentPackets.EnqueueValue(newValues.Value.SentPackets);
            SentBytes.EnqueueValue(newValues.Value.SentBytes);
            SendErrors.EnqueueValue(newValues.Value.SendErrors);
            RecvPackets.EnqueueValue(newValues.Value.RecvPackets);
            RecvBytes.EnqueueValue(newValues.Value.RecvBytes);
            RecvErrors.EnqueueValue(newValues.Value.RecvErrors);
            Collisions.EnqueueValue(newValues.Value.Collisions);

            return true;
        }
    }
}
