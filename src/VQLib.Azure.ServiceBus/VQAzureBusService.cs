using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using VQLib.Azure.ServiceBus.Model;
using VQLib.Util;

namespace VQLib.Azure.ServiceBus
{
    public class VQAzureBusService<T> : IVQAzureBusService<T>, IDisposable, IAsyncDisposable
    {
        private readonly VQServiceBusConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private ServiceBusClient? _queueClient;
        private ServiceBusProcessor? _processor;

        public VQAzureBusService(VQServiceBusConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        private ServiceBusClient GetClient()
        {
            if (_queueClient == null)
            {
                if (string.IsNullOrWhiteSpace(_configuration.QueueNameOrTopic))
                    throw new ArgumentNullException(nameof(_configuration.QueueNameOrTopic));

                _queueClient = new ServiceBusClient(_configuration.ConnectionString ?? throw new ArgumentNullException(nameof(_configuration.ConnectionString)));
            }

            return _queueClient;
        }

        private ServiceBusProcessor GetProcessor()
        {
            if (_processor == null)
            {
                var processorOptions = new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = _configuration.MaxConcurrentProcess,
                    AutoCompleteMessages = _configuration.AutoCompleteMessages,
                };

                if (!string.IsNullOrWhiteSpace(_configuration.SubscriptionName))
                {
                    _processor = GetClient().CreateProcessor(_configuration.QueueNameOrTopic, _configuration.SubscriptionName, processorOptions);
                }
                else
                {
                    _processor = GetClient().CreateProcessor(_configuration.QueueNameOrTopic, processorOptions);
                }
            }

            return _processor;
        }

        public async Task Enqueue(VQAzureQueueMessageBus<T> data, int delayInSeconds = 0)
        {
            var client = GetClient();

            var json = data.ToJson();
            var message = new ServiceBusMessage(json)
            {
                ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddSeconds(delayInSeconds)
            };

            await using var sender = client.CreateSender(_configuration.QueueNameOrTopic);
            await sender.SendMessageAsync(message);
        }

        public async Task Enqueue(IEnumerable<VQAzureQueueMessageBus<T>> dataList, int delayInSeconds = 0)
        {
            if (dataList == null || !dataList.Any())
                return;

            var client = GetClient();
            await using var sender = client.CreateSender(_configuration.QueueNameOrTopic);

            var list = new List<ServiceBusMessageBatch>()
            {
                await sender.CreateMessageBatchAsync()
            };
            foreach (var item in dataList)
            {
                var message = new ServiceBusMessage(item.ToJson())
                {
                    ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddSeconds(delayInSeconds),
                };

                if (list[^0].TryAddMessage(message))
                    continue;

                list.Add(await sender.CreateMessageBatchAsync());
                if (list[^0].TryAddMessage(message))
                    continue;

                throw new Exception("Message don't added to batch!");
            }

            foreach (var batch in list)
            {
                using var batchUsing = batch;
                await sender.SendMessagesAsync(batchUsing);
            }
        }

        public void Process(
            Func<ProcessMessageEventArgs, VQAzureQueueMessageBus<T>?, IServiceScope, Task> actionProcess,
            Func<ProcessErrorEventArgs, IServiceScope, Task> actionException)
        {
            var processor = GetProcessor();

            processor.ProcessMessageAsync += async (args) =>
            {
                var msg = args.Message.Body.ToString().FromJson<VQAzureQueueMessageBus<T>>();
                using var scope = _serviceProvider.CreateScope();
                await actionProcess(args, msg, scope);
            };

            processor.ProcessErrorAsync += async (args) =>
            {
                using var scope = _serviceProvider.CreateScope();
                await actionException(args, scope);
            };

            processor.StartProcessingAsync();
        }

        public void Dispose()
        {
            DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (_queueClient != null)
                await _queueClient.DisposeAsync();

            if (_processor != null)
                await _processor.DisposeAsync();
        }
    }
}