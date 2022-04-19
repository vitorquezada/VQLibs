using VQLib.Aws.Sqs.Model;

namespace VQLib.Aws.Sqs
{
    public interface IVQSqsQueueService
    {
        Task Enqueue<T>(VQSqsQueueMessage<T> data);

        Task Enqueue<T>(IEnumerable<VQSqsQueueMessage<T>> dataList);

        Task<List<T?>> Dequeue<T>(string queueName);

        Task<List<VQSqsQueueResponse<T?>>> DequeueConfirmation<T>(string queueName, TimeSpan visibilityTimeout, int maxNumberOfMessages = 10, TimeSpan? waitTime = null);

        Task Confirm(VQSqsQueueResponse response);

        Task ConfirmMany(IEnumerable<VQSqsQueueResponse> response);

        Task ChangeVisibilityTimeout(VQSqsQueueResponse receipt, TimeSpan visibilityTimeout);

        Task ChangeVisibilityManyTimeout(IEnumerable<VQSqsQueueResponse> receipt, TimeSpan visibilityTimeout);
    }
}