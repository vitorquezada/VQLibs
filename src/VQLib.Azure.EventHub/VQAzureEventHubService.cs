using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using VQLib.Azure.EventHub.Model;
using VQLib.Util;

namespace VQLib.Azure.EventHub
{
    public class VQAzureEventHubService<T> : IVQAzureEventHubService<T>, IDisposable, IAsyncDisposable
    {
        private readonly VQAzureEventHubConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private EventProcessorClient? _processor;
        private EventHubProducerClient? _producer;

        public VQAzureEventHubService(VQAzureEventHubConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (_producer != null)
                await _producer.DisposeAsync();

            if (_processor != null && _processor.IsRunning)
                _processor.StopProcessing();
        }

        public async Task Enqueue(T data, int delayInSeconds = 0)
        {
            var producer = GetProducer();

            var json = data.ToJson();
            using var eventBatch = await producer.CreateBatchAsync();
            if (!eventBatch.TryAdd(new EventData(json)))
            {
                throw new ArgumentException($"Data is is too large for the batch and cannot be sent.");
            }

            await producer.SendAsync(eventBatch);
        }

        public async Task Enqueue(IEnumerable<T> dataList, int delayInSeconds = 0)
        {
            if (dataList == null || !dataList.Any())
                return;

            var producer = GetProducer();

            var list = new List<EventDataBatch>()
            {
                await producer.CreateBatchAsync()
            };

            foreach (var item in dataList)
            {
                var message = new EventData(item.ToJson());

                if (list[^1].TryAdd(message))
                    continue;

                list.Add(await producer.CreateBatchAsync());
                if (list[^1].TryAdd(message))
                    continue;

                throw new Exception("Message don't added to batch!");
            }

            foreach (var batch in list)
            {
                using var batchUsing = batch;
                await producer.SendAsync(batchUsing);
            }
        }

        public async Task Process(
            Func<ProcessEventArgs, T?, IServiceScope, Task> actionProcess,
            Func<ProcessErrorEventArgs, IServiceScope, Task> actionException)
        {
            var processor = GetProcessor();

            processor.ProcessEventAsync += async (args) =>
            {
                var msg = Encoding.UTF8.GetString(args.Data.EventBody.ToArray()).FromJson<T>();
                using var scope = _serviceProvider.CreateScope();
                await actionProcess(args, msg, scope);
            };

            processor.ProcessErrorAsync += async (args) =>
            {
                using var scope = _serviceProvider.CreateScope();
                await actionException(args, scope);
            };

            await processor.StartProcessingAsync();
        }

        private EventProcessorClient GetProcessor()
        {
            if (_processor == null)
            {
                string consumerGroup = _configuration.ConsumerGroupName ?? EventHubConsumerClient.DefaultConsumerGroupName;

                var storageClient = new BlobContainerClient(_configuration.BlobStorageConnectionString, _configuration.BlobContainerName);

                _processor = new EventProcessorClient(storageClient, consumerGroup, _configuration.EventHubConnectionString, _configuration.EventHubName);
            }

            return _processor;
        }

        private EventHubProducerClient GetProducer()
        {
            if (_producer == null)
            {
                _producer = new EventHubProducerClient(_configuration.EventHubConnectionString, _configuration.EventHubName);
            }

            return _producer;
        }
    }
}