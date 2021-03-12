namespace Excogitated.ServiceBus.Azure
{
    internal class TransportSettings
    {
        public string ConnectionString { get; set; }
        public int PrefetchCount { get; set; } = 1000;
        public double MaxAutoRenewHours { get; set; } = 1;
    }
}
