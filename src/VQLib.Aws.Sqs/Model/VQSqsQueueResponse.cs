namespace VQLib.Aws.Sqs.Model
{
    public class VQSqsQueueResponse<T> : VQSqsQueueResponse
    {
        public T Data { get; set; }
    }

    public abstract class VQSqsQueueResponse
    {
        public string Receipt { get; set; }

        public string QueueName { get; set; }
    }
}