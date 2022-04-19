using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text;
using VQLib.Aws.Model;
using VQLib.Aws.Sqs.Model;
using VQLib.Util;

namespace VQLib.Aws.Sqs
{
    public class VQSqsQueueService : IVQSqsQueueService, IDisposable
    {
        private const int MAX_BATCH_SIZE = 10;

        private Dictionary<string, string> _cacheQueueUrl = new Dictionary<string, string>();

        private readonly VQAwsConfig _config;

        private IAmazonSQS? _client;

        private IAmazonSQS GetClient => _client ??= !string.IsNullOrWhiteSpace(_config.AccessKey) && !string.IsNullOrWhiteSpace(_config.SecretKey)
                    ? new AmazonSQSClient(_config.AccessKey, _config.SecretKey, _config.Region)
                    : new AmazonSQSClient(_config.Region);

        public VQSqsQueueService(VQAwsConfig config)
        {
            _config = config;
        }

        public async Task ChangeVisibilityManyTimeout(IEnumerable<VQSqsQueueResponse> receipt, TimeSpan visibilityTimeout)
        {
            var client = GetClient;

            var requestByQueue = receipt.GroupBy(x => x.QueueName).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var itemsByQueueName in requestByQueue)
            {
                var obj = new ChangeMessageVisibilityBatchRequest
                {
                    Entries = itemsByQueueName.Value.Select((x, i) => new ChangeMessageVisibilityBatchRequestEntry
                    {
                        Id = i.ToString(),
                        ReceiptHandle = x.QueueName,
                        VisibilityTimeout = Convert.ToInt32(visibilityTimeout.TotalSeconds),
                    }).ToList(),
                    QueueUrl = await GetQueueUrl(client, itemsByQueueName.Key),
                };
                var response = await client.ChangeMessageVisibilityBatchAsync(obj);
            }
        }

        private async Task<string> GetQueueUrl(IAmazonSQS client, string queueName)
        {
            if (_cacheQueueUrl.ContainsKey(queueName))
            {
                var response = await client.GetQueueUrlAsync(new GetQueueUrlRequest
                {
                    QueueName = queueName
                });

                _cacheQueueUrl.TryAdd(queueName, response.QueueUrl);
            }

            return _cacheQueueUrl[queueName];
        }

        public async Task ChangeVisibilityTimeout(VQSqsQueueResponse receipt, TimeSpan visibilityTimeout)
        {
            var client = GetClient;

            var obj = new ChangeMessageVisibilityRequest
            {
                ReceiptHandle = receipt.Receipt,
                QueueUrl = await GetQueueUrl(client, receipt.QueueName),
                VisibilityTimeout = Convert.ToInt32(visibilityTimeout.TotalSeconds),
            };

            var response = await client.ChangeMessageVisibilityAsync(obj);
        }

        public async Task Confirm(VQSqsQueueResponse response)
        {
            await Confirm(response.Receipt, response.QueueName);
        }

        private async Task Confirm(string receipt, string queueName)
        {
            var client = GetClient;

            var delete = new DeleteMessageRequest
            {
                ReceiptHandle = receipt,
                QueueUrl = await GetQueueUrl(client, queueName),
            };

            await client.DeleteMessageAsync(delete);
        }

        public async Task ConfirmMany(IEnumerable<VQSqsQueueResponse> response)
        {
            var responseByQueueName = response.GroupBy(x => x.QueueName).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var itemsByQueueName in responseByQueueName)
            {
                await ConfirmMany(itemsByQueueName.Value.Select(x => x.Receipt).ToHashSet(), itemsByQueueName.Key);
            }
        }

        public async Task ConfirmMany(IEnumerable<string> receipt, string queueName)
        {
            var client = GetClient;

            var delete = new DeleteMessageBatchRequest
            {
                Entries = receipt.Select((x, i) => new DeleteMessageBatchRequestEntry
                {
                    ReceiptHandle = x,
                    Id = i.ToString(),
                }).ToList(),
                QueueUrl = await GetQueueUrl(client, queueName),
            };

            await client.DeleteMessageBatchAsync(delete);
        }

        public async Task<List<T?>> Dequeue<T>(string queueName)
        {
            var messageResponses = await GetMessages(queueName);

            var response = new List<T?>();
            foreach (var message in messageResponses.Messages)
            {
                response.Add(message.Body.FromJson<T>());
                await Confirm(message.ReceiptHandle, queueName);
            }

            return response;
        }

        public Task<List<VQSqsQueueResponse<T?>>> DequeueConfirmation<T>(string queueName, TimeSpan visibilityTimeout = default, int maxNumberOfMessages = 10, TimeSpan? waitTime = null)
        {
            var visibilityTimeoutSeconds = visibilityTimeout.Ticks > 0
                ? Convert.ToInt32(visibilityTimeout.TotalSeconds)
                : 3600;
            var waitTimeInSeconds = waitTime.HasValue
                ? Convert.ToInt32(waitTime.Value.TotalSeconds)
                : default(int?);
            return DequeueConfirmation<T>(queueName, visibilityTimeoutSeconds, maxNumberOfMessages, waitTimeInSeconds);
        }

        private async Task<List<VQSqsQueueResponse<T?>>> DequeueConfirmation<T>(string queueName, int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null)
        {
            var messageResponses = await GetMessages(queueName, visibilityTimeoutSeconds, maxNumberOfMessages, waitTimeSeconds);

            var response = new List<VQSqsQueueResponse<T?>>();
            foreach (var message in messageResponses.Messages)
            {
                var document = message.Body.FromJson<T>();
                response.Add(new VQSqsQueueResponse<T?>
                {
                    Receipt = message.ReceiptHandle,
                    Data = document,
                    QueueName = queueName,
                });
            }
            return response;
        }

        public async Task Enqueue<T>(VQSqsQueueMessage<T> message)
        {
            var dataSerialized = message.Data.ToJson();
            CheckDataSize(dataSerialized);

            using var client = GetClient;
            var queueUrl = await EnsureCreateAndReturnUrlQueue(client, message.QueueName);

            var messageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = dataSerialized
            };

            var response = await client.SendMessageAsync(messageRequest);
        }

        public async Task Enqueue<T>(IEnumerable<VQSqsQueueMessage<T>> dataList)
        {
            if (dataList == null || !dataList.Any())
                return;

            var dataListDataSerialized = dataList
                .Select(x => new { x.QueueName, DataSerialized = x.Data.ToJson() });

            foreach (var data in dataListDataSerialized)
            {
                CheckDataSize(data.DataSerialized);
            }

            var dataListSeparated = dataListDataSerialized
                .GroupBy(x => x.QueueName)
                .Select(x => new { QueueName = x.Key, MessageList = x.Select(x => x.DataSerialized).ToList() });

            var dataWithClientList = await Task.WhenAll(dataListSeparated.Select(async (x) =>
            {
                var client = GetClient;
                var queueUrl = await EnsureCreateAndReturnUrlQueue(client, x.QueueName);

                return new
                {
                    x.MessageList,
                    Client = client,
                    QueueUrl = queueUrl,
                };
            }));

            var dataBatch = dataWithClientList
                .SelectMany(x => x.MessageList.Chunk(MAX_BATCH_SIZE), (x, y) => new { x.Client, x.QueueUrl, Batch = y });

            var messageBatchSendRequest = await Task.WhenAll(dataBatch.Select(async b =>
            {
                var request = new SendMessageBatchRequest
                {
                    QueueUrl = b.QueueUrl,
                    Entries = b.Batch.Select((m, i) => new SendMessageBatchRequestEntry
                    {
                        Id = i.ToString(),
                        MessageBody = m,
                    }).ToList()
                };
                return await b.Client.SendMessageBatchAsync(request);
            }));

            foreach (var client in dataWithClientList)
            {
                client.Client.Dispose();
            }
        }

        private async Task<ReceiveMessageResponse> GetMessages(string queueName, int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null)
        {
            var client = GetClient;

            var messageRequest = new ReceiveMessageRequest()
            {
                QueueUrl = await GetQueueUrl(client, queueName),
                MaxNumberOfMessages = maxNumberOfMessages,
                VisibilityTimeout = visibilityTimeoutSeconds,
            };

            if (waitTimeSeconds.HasValue && waitTimeSeconds.Value > 0)
                messageRequest.WaitTimeSeconds = waitTimeSeconds.Value;

            var messageResponses = await client.ReceiveMessageAsync(messageRequest);

            return messageResponses;
        }

        private void CheckDataSize(string data)
        {
            int MAX_SIZE_KB = 256 * 1024;

            var dataSize = Encoding.ASCII.GetByteCount(data);
            if (dataSize > MAX_SIZE_KB)
                throw new Exception("Impossible to queue data. The item should have a maximum of 256kb in size. Data: " + data);
        }

        private static async Task<string> EnsureCreateAndReturnUrlQueue(
            IAmazonSQS client,
            string queueName)
        {
            var queueList = await client.ListQueuesAsync(queueName);

            var existsQueue = queueList != null && queueList.QueueUrls != null && queueList.QueueUrls.Any(x => x == queueName);
            if (!existsQueue)
            {
                var queueRequest = new CreateQueueRequest
                {
                    QueueName = queueName,
                    Attributes = new Dictionary<string, string>
                    {
                        { QueueAttributeName.MessageRetentionPeriod, TimeSpan.FromDays(14).TotalSeconds.ToString() },
                    },
                };

                _ = await client.CreateQueueAsync(queueRequest);
            }

            var queueUrlResponse = await client.GetQueueUrlAsync(queueName);
            return queueUrlResponse.QueueUrl;
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}