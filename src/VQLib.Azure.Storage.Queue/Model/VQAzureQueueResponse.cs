namespace VQLib.Azure.Storage.Queue.Model
{
    public class VQAzureQueueResponse<T> : VQAzureQueueResponse
    {
        public VQAzureQueueResponse(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
    }

    public class VQAzureQueueResponse
    {
        public string? MessageId { get; set; }

        public string? PopReceipt { get; set; }

        public string? QueueName { get; set; }
    }
}