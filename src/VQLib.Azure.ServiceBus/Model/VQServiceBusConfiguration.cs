namespace VQLib.Azure.ServiceBus.Model
{
    public class VQServiceBusConfiguration
    {
        public string? ConnectionString { get; set; }

        public string? QueueNameOrTopic { get; set; }

        public string? SubscriptionName { get; set; }

        public int MaxConcurrentProcess { get; set; } = 1;

        public bool AutoCompleteMessages { get; set; } = false;
    }
}