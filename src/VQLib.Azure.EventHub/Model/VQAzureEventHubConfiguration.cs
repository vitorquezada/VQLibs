namespace VQLib.Azure.EventHub.Model
{
    public class VQAzureEventHubConfiguration
    {
        public string? BlobContainerName { get; set; }

        public string? BlobStorageConnectionString { get; set; }

        public string? ConsumerGroupName { get; set; }

        public string? EventHubConnectionString { get; set; }

        public string? EventHubName { get; set; }
    }
}