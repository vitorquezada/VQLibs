namespace VQLib.Azure.Storage.Queue.Model
{
    public class VQAzureQueueMessage<T>
    {
        public T Data { get; set; }

        public string QueueName { get; set; }
    }
}