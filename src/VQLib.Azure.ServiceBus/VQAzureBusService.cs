﻿using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using VQLib.Azure.ServiceBus.Model;
using VQLib.Util;

namespace VQLib.Azure.ServiceBus
{
    public class VQAzureBusService<T> : IVQAzureBusService<T>, IDisposable, IAsyncDisposable
    {
        private readonly VQServiceBusConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private ServiceBusProcessor? _processor;
        private ServiceBusClient? _queueClient;

        public VQAzureBusService(VQServiceBusConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public async Task<QueueProperties> CreateQueue(CreateQueueOptions model)
        {
            var client = GetAdministrationClient();
            var existResult = await client.CreateQueueAsync(model);
            return existResult.Value;
        }

        public async Task<QueueProperties> CreateUpdateQueue(CreateQueueOptions properties)
        {
            if (!string.IsNullOrWhiteSpace(_configuration.SubscriptionName))
                throw new ArgumentException(
                    $"{nameof(_configuration.SubscriptionName)} present means a Topic/Subscription setting instead of Queue.",
                    nameof(_configuration.SubscriptionName));

            if (properties.Name != _configuration.QueueNameOrTopic)
                throw new ArgumentException($"Argument {nameof(properties.Name)} must be equals {nameof(_configuration.QueueNameOrTopic)}", nameof(properties.Name));

            var client = GetAdministrationClient();

            var existResult = await client.QueueExistsAsync(_configuration.QueueNameOrTopic);
            if (existResult.Value)
            {
                var existedQueue = await client.GetQueueAsync(_configuration.QueueNameOrTopic);
                var propertiesUpdate = existedQueue?.Value ?? throw new ArgumentNullException();

                properties.CopyPropertiesTo(propertiesUpdate, new[] { nameof(properties.UserMetadata) });
                if (properties.UserMetadata != null)
                    propertiesUpdate.UserMetadata = properties.UserMetadata;

                var responseUpdate = await client.UpdateQueueAsync(propertiesUpdate);
                return responseUpdate.Value;
            }
            else
            {
                var responseCreate = await client.CreateQueueAsync(properties);
                return responseCreate.Value;
            }
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

        public virtual async Task Enqueue(T data, int delayInSeconds = 0)
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

        public virtual async Task Enqueue(IEnumerable<T> dataList, int delayInSeconds = 0)
        {
            if (dataList == null || !dataList.Any())
                return;

            var client = GetClient();
            await using var sender = client.CreateSender(_configuration.QueueNameOrTopic);

            long MAX_BYTES = Convert.ToInt64(Math.Round(0.8 * 256 * 1024));

            var batchOptions = new CreateMessageBatchOptions
            {
                MaxSizeInBytes = MAX_BYTES
            };

            var list = new List<ServiceBusMessageBatch>()
            {
                await sender.CreateMessageBatchAsync(batchOptions)
            };
            foreach (var item in dataList)
            {
                var message = new ServiceBusMessage(item.ToJson())
                {
                    ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddSeconds(delayInSeconds),
                };

                if (list[^1].TryAddMessage(message))
                    continue;

                list.Add(await sender.CreateMessageBatchAsync(batchOptions));
                if (list[^1].TryAddMessage(message))
                    continue;

                throw new Exception("Message don't added to batch!");
            }

            foreach (var batch in list)
            {
                try
                {
                    using var batchUsing = batch;
                    await sender.SendMessagesAsync(batchUsing);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<QueueProperties?> GetQueueProperties()
        {
            var client = GetAdministrationClient();
            var existResult = await client.QueueExistsAsync(_configuration.QueueNameOrTopic);
            if (!existResult.Value)
                return null;

            var existedQueue = await client.GetQueueAsync(_configuration.QueueNameOrTopic);
            return existedQueue?.Value ?? throw new ArgumentNullException();
        }

        public async Task<QueueRuntimeProperties> GetRuntimeProperties()
        {
            var client = GetAdministrationClient();
            var queueResponse = await client.GetQueueRuntimePropertiesAsync(_configuration.QueueNameOrTopic);
            return queueResponse.Value;
        }

        public virtual void Process(
            Func<ProcessMessageEventArgs, T?, IServiceScope, Task> actionProcess,
            Func<ProcessErrorEventArgs, IServiceScope, Task> actionException)
        {
            var processor = GetProcessor();

            processor.ProcessMessageAsync += async (args) =>
            {
                var msg = args.Message.Body.ToString().FromJson<T>();
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

        public async Task<QueueProperties> UpdateQueue(QueueProperties model)
        {
            var client = GetAdministrationClient();
            var existResult = await client.UpdateQueueAsync(model);
            return existResult.Value;
        }

        private ServiceBusAdministrationClient GetAdministrationClient()
        {
            var connectionString = _configuration.ConnectionString ?? throw new ArgumentNullException(nameof(_configuration.ConnectionString));
            return new ServiceBusAdministrationClient(connectionString);
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
    }
}