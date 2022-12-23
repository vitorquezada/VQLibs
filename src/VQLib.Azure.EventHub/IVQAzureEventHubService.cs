using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.DependencyInjection;

namespace VQLib.Azure.EventHub
{
    public interface IVQAzureEventHubService<T>
    {
        Task Enqueue(T data, int delayInSeconds = 0);

        Task Enqueue(IEnumerable<T> dataList, int delayInSeconds = 0);

        Task Process(
            Func<ProcessEventArgs, T?, IServiceScope, Task> actionProcess,
            Func<ProcessErrorEventArgs, IServiceScope, Task> actionException);
    }
}