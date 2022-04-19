using VQLib.Azure.Storage.Queue.Model;

namespace VQLib.Azure.Storage.Queue
{
    public interface IVQAzureQueueService
    {
        Task Enqueue<T>(VQAzureQueueMessage<T> data, int delayInSeconds = 0);

        Task Enqueue<T>(IEnumerable<VQAzureQueueMessage<T>> dataList, int delayInSeconds = 0);

        Task<List<T?>> Dequeue<T>(string queueName, int maxNumberOfMesages = 32);

        Task<List<VQAzureQueueResponse<T?>>> DequeueConfirmation<T>(string queueName, TimeSpan visibilityTimeout = default, int maxNumberOfMessages = 32);

        Task Confirm(VQAzureQueueResponse response);

        Task ConfirmMany(IEnumerable<VQAzureQueueResponse> response);

        Task ChangeVisibilityTimeout(VQAzureQueueResponse receipt, TimeSpan visibilityTimeout);

        Task ChangeVisibilityManyTimeout(IEnumerable<VQAzureQueueResponse> receipt, TimeSpan visibilityTimeout);
    }
}