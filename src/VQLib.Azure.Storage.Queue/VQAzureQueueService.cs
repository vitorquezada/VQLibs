using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text;
using VQLib.Azure.Storage.Queue.Model;
using VQLib.Util;

namespace VQLib.Azure.Storage.Queue
{
    public class VQAzureQueueService : IVQAzureQueueService
    {
        private static readonly TimeSpan DEFAULT_TIMEOUT_VISIBILITY = TimeSpan.FromMinutes(1);
        private QueueClient? _client;
        private readonly VQAzureStorageConfig _config;

        private Dictionary<string, QueueClient> _cacheQueueClient = new Dictionary<string, QueueClient>();

        public VQAzureQueueService(VQAzureStorageConfig config)
        {
            _config = config;
        }

        private async Task<QueueClient> GetClient(string? queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            if (string.IsNullOrWhiteSpace(_config?.StorageConnectionString))
                throw new ArgumentNullException(nameof(_config.StorageConnectionString));

            if (!_cacheQueueClient.ContainsKey(queueName))
            {
                _cacheQueueClient[queueName] = new QueueClient(_config.StorageConnectionString, queueName);
                await _cacheQueueClient[queueName].CreateIfNotExistsAsync();
            }

            return _cacheQueueClient[queueName];
        }

        public async Task ChangeVisibilityTimeout(VQAzureQueueResponse receipt, TimeSpan visibilityTimeout)
        {
            var client = await GetClient(receipt.QueueName);

            _ = await client.UpdateMessageAsync(receipt.MessageId, receipt.PopReceipt, visibilityTimeout: visibilityTimeout);
        }

        public async Task ChangeVisibilityManyTimeout(IEnumerable<VQAzureQueueResponse> receipt, TimeSpan visibilityTimeout)
        {
            var tasks = receipt.Select(async x => await ChangeVisibilityTimeout(x, visibilityTimeout));
            await Task.WhenAll(tasks);
        }

        public async Task Confirm(VQAzureQueueResponse receipt)
        {
            var client = await GetClient(receipt.QueueName);

            using var response = await client.DeleteMessageAsync(receipt.MessageId, receipt.PopReceipt);
        }

        public async Task ConfirmMany(IEnumerable<VQAzureQueueResponse> response)
        {
            var tasks = response.Select(async x => await Confirm(x));
            await Task.WhenAll(tasks);
        }

        public async Task<List<T?>> Dequeue<T>(string queueName, int maxNumberOfMesages = 32)
        {
            var messages = await GetMessagesInternal<T>(queueName, TimeSpan.FromMinutes(1), maxNumberOfMesages);

            await ConfirmMany(messages);

            return messages.Select(x => x.Data).ToList();
        }

        public async Task<List<VQAzureQueueResponse<T?>>> DequeueConfirmation<T>(string queueName, TimeSpan visibilityTimeout = default, int maxNumberOfMessages = 32)
        {
            if (visibilityTimeout == default)
                visibilityTimeout = DEFAULT_TIMEOUT_VISIBILITY;

            return await GetMessagesInternal<T>(queueName, visibilityTimeout, maxNumberOfMessages);
        }

        private async Task<List<VQAzureQueueResponse<T?>>> GetMessagesInternal<T>(string queueName, TimeSpan visibilityTimeout, int maxNumberOfMessages)
        {
            var client = await GetClient(queueName);

            var messages = await client.ReceiveMessagesAsync(maxNumberOfMessages, visibilityTimeout);

            var response = new List<VQAzureQueueResponse<T?>>();

            foreach (var message in messages.Value)
            {
                var messageObj = message.Body.ToString().FromJson<T>();

                response.Add(new VQAzureQueueResponse<T?>(messageObj)
                {
                    QueueName = queueName,
                    MessageId = message.MessageId,
                    PopReceipt = message.PopReceipt,
                });
            }

            return response.ToList();
        }

        public async Task Enqueue<T>(VQAzureQueueMessage<T> data, int delayInSeconds = 0)
        {
            var delay = TimeSpan.FromSeconds(delayInSeconds);
            CheckDelay(delay);

            var dataSerialized = data.Data.ToJson();
            CheckDataSize(dataSerialized);

            _ = await EnqueueInternal(dataSerialized, data.QueueName, delay);
        }

        public async Task Enqueue<T>(IEnumerable<VQAzureQueueMessage<T>> dataList, int delayInSeconds = 0)
        {
            var delay = TimeSpan.FromSeconds(delayInSeconds);
            CheckDelay(delay);

            var enqueueTask = dataList.Select(async x =>
            {
                var dataSerialized = x.Data.ToJson();
                CheckDataSize(dataSerialized);

                return await EnqueueInternal(dataSerialized, x.QueueName, delay);
            });

            await Task.WhenAll(enqueueTask);
        }

        private async Task<Response<SendReceipt>> EnqueueInternal(string message, string queueName, TimeSpan delay)
        {
            var queue = await GetClient(queueName);
            return await queue.SendMessageAsync(message, visibilityTimeout: delay);
        }

        private static void CheckDataSize(string data)
        {
            int MAX_SIZE_KB = 64 * 1024;

            var dataSize = Encoding.UTF8.GetByteCount(data);
            if (dataSize > MAX_SIZE_KB)
                throw new Exception("Impossible to queue data. The item should have a maximum of 64kb in size. Data: " + data);
        }

        private static void CheckDelay(TimeSpan delay)
        {
            if (delay > TimeSpan.FromDays(7))
                throw new ArgumentOutOfRangeException("Impossible to enqueue item. The maximum value for delay is 7 days");
        }
    }
}