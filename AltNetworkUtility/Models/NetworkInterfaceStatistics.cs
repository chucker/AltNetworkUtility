namespace AltNetworkUtility.Models
{
    public class NetworkInterfaceStatistics
    {
        public ulong SentPackets { get; set; }
        public ulong SendErrors { get; set; }
        public ulong RecvPackets { get; set; }
        public ulong RecvErrors { get; set; }
        public ulong Collisions { get; set; }
    }
}
