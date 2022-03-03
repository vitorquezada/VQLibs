using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VQLib.Queue.Model;

namespace VQLib.Queue
{
    public interface IVQSqsQueueService<T>
    {
        IVQSqsQueueService<T> Config(string queueName, int messageRetentionPeriodDays = 14, bool createDeadQueue = true, int deadQueueAttempts = 10);

        Task Enqueue(T data, int delayInSeconds = 0);

        Task Enqueue(IEnumerable<T> dataList, int delayInSeconds = 0);

        Task<List<T>> Dequeue();

        Task<List<QueueResponse<T>>> DequeueConfirmation(int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null);

        Task<List<QueueResponse<T>>> DequeueConfirmation(TimeSpan visibilityTimeout, int maxNumberOfMessages = 10, TimeSpan? waitTime = null);

        Task Confirm(QueueResponse response);

        Task ConfirmMany(IEnumerable<QueueResponse> response);

        Task Confirm(string receipt);

        Task ConfirmMany(IEnumerable<string> response);

        Task ChangeVisibilityTimeout(string receipt, int visibilityTimeoutSeconds = 3600);

        Task ChangeVisibilityManyTimeout(IEnumerable<string> receipt, int visibilityTimeoutSeconds = 3600);
    }
}