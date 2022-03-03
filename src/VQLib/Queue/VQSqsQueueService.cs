using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VQLib.AwsCloud.Model;
using VQLib.Queue.Model;
using VQLib.Util;

namespace VQLib.Queue
{
    public class VQSqsQueueService<T> : IVQSqsQueueService<T>, IDisposable
    {
        private readonly VQAwsConfigModel _configModel;

        private string _queueName;
        private string _queueUrl;
        private TimeSpan _messageRetentionPeriod;
        private bool _createDeadQueue;
        private int _deadQueueAttempts;

        private IAmazonSQS _client;

        public VQSqsQueueService(VQAwsConfigModel configModel)
        {
            _configModel = configModel;
        }

        public IVQSqsQueueService<T> Config(string queueName, int messageRetentionPeriodDays = 14, bool createDeadQueue = true, int deadQueueAttempts = 10)
        {
            _queueName = queueName;
            _messageRetentionPeriod = TimeSpan.FromDays(messageRetentionPeriodDays);
            _createDeadQueue = createDeadQueue;
            _deadQueueAttempts = deadQueueAttempts;
            return this;
        }

        public async Task ChangeVisibilityManyTimeout(IEnumerable<string> receipt, int visibilityTimeoutSeconds = 3600)
        {
            var obj = new ChangeMessageVisibilityBatchRequest
            {
                Entries = receipt.Select((x, i) => new ChangeMessageVisibilityBatchRequestEntry
                {
                    Id = i.ToString(),
                    ReceiptHandle = x,
                    VisibilityTimeout = visibilityTimeoutSeconds
                }).ToList(),
                QueueUrl = _queueUrl,
            };

            var client = await GetClient();
            var response = await client.ChangeMessageVisibilityBatchAsync(obj);
        }

        public async Task ChangeVisibilityTimeout(string receipt, int visibilityTimeoutSeconds = 3600)
        {
            var obj = new ChangeMessageVisibilityRequest
            {
                ReceiptHandle = receipt,
                QueueUrl = _queueUrl,
                VisibilityTimeout = visibilityTimeoutSeconds,
            };

            var client = await GetClient();
            var response = await client.ChangeMessageVisibilityAsync(obj);
        }

        public async Task Confirm(QueueResponse response)
        {
            await Confirm(response.Receipt);
        }

        public async Task Confirm(string receipt)
        {
            var delete = new DeleteMessageRequest
            {
                ReceiptHandle = receipt,
                QueueUrl = _queueUrl,
            };

            var client = await GetClient();
            await client.DeleteMessageAsync(delete);
        }

        public async Task ConfirmMany(IEnumerable<QueueResponse> response)
        {
            await ConfirmMany(response.Select(x => x.Receipt).ToHashSet());
        }

        public async Task ConfirmMany(IEnumerable<string> receipt)
        {
            var delete = new DeleteMessageBatchRequest
            {
                Entries = receipt.Select((x, i) => new DeleteMessageBatchRequestEntry
                {
                    ReceiptHandle = x,
                    Id = i.ToString(),
                }).ToList(),
                QueueUrl = _queueUrl,
            };

            var client = await GetClient();
            await client.DeleteMessageBatchAsync(delete);
        }

        public async Task<List<T>> Dequeue()
        {
            var messageResponses = await GetMessages();

            var response = new List<T>();
            foreach (var message in messageResponses.Messages)
            {
                response.Add(message.Body.FromJson<T>());
                await Confirm(message.ReceiptHandle);
            }

            return response;
        }

        public Task<List<QueueResponse<T>>> DequeueConfirmation(TimeSpan visibilityTimeout = default, int maxNumberOfMessages = 10, TimeSpan? waitTime = null)
        {
            var visibilityTimeoutSeconds = visibilityTimeout.Ticks > 0
                ? Convert.ToInt32(visibilityTimeout.TotalSeconds)
                : 3600;
            var waitTimeInSeconds = waitTime.HasValue
                ? Convert.ToInt32(waitTime.Value.TotalSeconds)
                : default(int?);
            return DequeueConfirmation(visibilityTimeoutSeconds, maxNumberOfMessages, waitTimeInSeconds);
        }

        public async Task<List<QueueResponse<T>>> DequeueConfirmation(int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null)
        {
            var messageResponses = await GetMessages(visibilityTimeoutSeconds, maxNumberOfMessages, waitTimeSeconds);

            var response = new List<QueueResponse<T>>();
            foreach (var message in messageResponses.Messages)
            {
                var document = message.Body.FromJson<T>();
                response.Add(new QueueResponse<T>
                {
                    Receipt = message.ReceiptHandle,
                    Data = document
                });
            }
            return response;
        }

        public async Task Enqueue(T data, int delayInSeconds = 0)
        {
            if (delayInSeconds > 900)
                throw new ArgumentOutOfRangeException("Impossible to enqueue item. The maximum value for delay is 900 (=15minutes)");

            var dataSerialized = data.ToJson();
            CheckDataSize(dataSerialized);

            var message = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                DelaySeconds = delayInSeconds,
                MessageBody = dataSerialized
            };

            var client = await GetClient();
            var response = await client.SendMessageAsync(message);
        }

        public async Task Enqueue(IEnumerable<T> dataList, int delayInSeconds = 0)
        {
            var request = new SendMessageBatchRequest
            {
                QueueUrl = _queueUrl,
                Entries = dataList.Select((x, index) => new SendMessageBatchRequestEntry
                {
                    Id = index.ToString(),
                    DelaySeconds = delayInSeconds,
                    MessageBody = x.ToJson(),
                }).ToList()
            };

            var client = await GetClient();
            var response = await client.SendMessageBatchAsync(request);
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }

        private async Task<ReceiveMessageResponse> GetMessages(int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null)
        {
            var messageRequest = new ReceiveMessageRequest()
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = maxNumberOfMessages,
                VisibilityTimeout = visibilityTimeoutSeconds,
            };

            if (waitTimeSeconds.GetValueOrDefault() > 0)
                messageRequest.WaitTimeSeconds = waitTimeSeconds.Value;

            var client = await GetClient();
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

        private async Task<IAmazonSQS> GetClient()
        {
            if (_client == null)
            {
                _client = _configModel.AccessKey.IsNotNullOrWhiteSpace() && _configModel.SecretKey.IsNotNullOrWhiteSpace()
                    ? new AmazonSQSClient(_configModel.AccessKey, _configModel.SecretKey, _configModel.Region)
                    : new AmazonSQSClient(_configModel.Region);

                var queueList = await _client.ListQueuesAsync(_queueName);

                var existsQueue = queueList.QueueUrls?.Any(x => x.EndsWith(_queueName)) ?? false;
                var deadQueueArn = await CreateAndGetArnDeadQueue(queueList, existsQueue);
                _queueUrl = await CreateAndReturnUrlQueue(existsQueue, deadQueueArn);
            }

            return _client;
        }

        private async Task<string> CreateAndGetArnDeadQueue(ListQueuesResponse queueList, bool existsQueue)
        {
            if (!_createDeadQueue)
                return null;

            var deadQueueName = $"{_queueName}-DEAD";

            var urlDeadQueue = queueList?.QueueUrls?.FirstOrDefault(x => x.EndsWith(deadQueueName)) ?? string.Empty;

            if (urlDeadQueue.IsNullOrWhiteSpace())
            {
                var queueDead = await _client.CreateQueueAsync(new CreateQueueRequest
                {
                    QueueName = deadQueueName,
                    Attributes = new Dictionary<string, string>
                    {
                        { QueueAttributeName.MessageRetentionPeriod, TimeSpan.FromDays(14).TotalSeconds.ToString() },
                    },
                });
                return queueDead.ResponseMetadata.Metadata[QueueAttributeName.QueueArn];
            }

            if (!existsQueue)
            {
                var url = queueList.QueueUrls.First(x => x.EndsWith(deadQueueName));

                var attr = await _client.GetQueueAttributesAsync(new GetQueueAttributesRequest
                {
                    QueueUrl = url,
                    AttributeNames = new List<string>
                    {
                        QueueAttributeName.QueueArn,
                    },
                });

                return attr.Attributes[QueueAttributeName.QueueArn];
            }

            return null;
        }

        private async Task<string> CreateAndReturnUrlQueue(bool existsQueue, string deadQueueArn)
        {
            if (!existsQueue)
            {
                var queueRequest = new CreateQueueRequest
                {
                    QueueName = _queueName,
                    Attributes = new Dictionary<string, string>
                    {
                        { QueueAttributeName.MessageRetentionPeriod, _messageRetentionPeriod.TotalSeconds.ToString() },
                    },
                };

                if (deadQueueArn.IsNotNullOrWhiteSpace())
                {
                    queueRequest.Attributes.Add(QueueAttributeName.RedrivePolicy, new
                    {
                        deadLetterTargetArn = deadQueueArn,
                        maxReceiveCount = _deadQueueAttempts,
                    }.ToJson());
                }

                _ = await _client.CreateQueueAsync(queueRequest);
            }

            var queueUrlResponse = await _client.GetQueueUrlAsync(_queueName);
            return queueUrlResponse.QueueUrl;
        }
    }
}