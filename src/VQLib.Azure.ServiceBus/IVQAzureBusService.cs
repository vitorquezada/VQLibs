﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using VQLib.Azure.ServiceBus.Model;

namespace VQLib.Azure.ServiceBus
{
    public interface IVQAzureBusService<T>
    {
        Task Enqueue(VQAzureQueueMessageBus<T> data, int delayInSeconds = 0);

        Task Enqueue(IEnumerable<VQAzureQueueMessageBus<T>> dataList, int delayInSeconds = 0);

        void Process(
            Func<ProcessMessageEventArgs, VQAzureQueueMessageBus<T>?, IServiceScope, Task> actionProcess,
            Func<ProcessErrorEventArgs, IServiceScope, Task> actionException);
    }
}