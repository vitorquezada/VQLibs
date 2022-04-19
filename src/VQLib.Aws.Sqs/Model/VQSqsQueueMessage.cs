namespace VQLib.Aws.Sqs.Model
{
    public class VQSqsQueueMessage<T>
    {
        public T Data { get; set; }

        public string QueueName { get; set; }
    }
}