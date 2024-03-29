﻿using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace VQLib.Azure.ServiceBus
{
    public interface IVQAzureBusService<T>
    {
        Task<QueueProperties> CreateQueue(CreateQueueOptions model);

        Task<QueueProperties> CreateUpdateQueue(CreateQueueOptions properties);

        Task Enqueue(T data, int delayInSeconds = 0);

        Task Enqueue(IEnumerable<T> dataList, int delayInSeconds = 0);

        Task<QueueProperties?> GetQueueProperties();

        Task<QueueRuntimeProperties> GetRuntimeProperties();

        void Process(
            Func<ProcessMessageEventArgs, T?, IServiceScope, Task> actionProcess,
            Func<ProcessErrorEventArgs, IServiceScope, Task> actionException);

        Task<QueueProperties> UpdateQueue(QueueProperties model);
    }
}