using System.Collections.Generic;
using System.Threading.Tasks;
using VQLib.Queue.Model;

namespace VQLib.Queue
{
    public interface IVQSqsQueueService<T>
    {
        Task Enqueue(T data, string queueName, int delayInSeconds = 0);

        Task Enqueue(List<T> dataList, string queueName, int delayInSeconds = 0);

        Task<List<T>> Dequeue(string queueName);

        Task<List<QueueResponse<T>>> DequeueConfirmation(string queueName, int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null);

        Task Confirm(QueueResponse response, string queueName);

        Task ConfirmMany(IEnumerable<QueueResponse> response, string queueName);

        Task Confirm(string receipt, string queueName);

        Task ConfirmMany(IEnumerable<string> response, string queueName);

        Task ChangeVisibilityTimeout(string receipt, string queueName, int visibilityTimeoutSeconds = 3600);

        Task ChangeVisibilityManyTimeout(IEnumerable<string> receipt, string queueName, int visibilityTimeoutSeconds = 3600);
    }
}