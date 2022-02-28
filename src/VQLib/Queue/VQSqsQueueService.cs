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

        private string QueueName;
        private string QueueUrl;

        private IAmazonSQS _client;

        public VQSqsQueueService(VQAwsConfigModel configModel)
        {
            _configModel = configModel;
        }

        public IVQSqsQueueService<T> Config(string queueName)
        {
            QueueName = queueName;
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
                QueueUrl = QueueUrl,
            };

            var client = await GetClient();
            var response = await client.ChangeMessageVisibilityBatchAsync(obj);
        }

        public async Task ChangeVisibilityTimeout(string receipt, int visibilityTimeoutSeconds = 3600)
        {
            var obj = new ChangeMessageVisibilityRequest
            {
                ReceiptHandle = receipt,
                QueueUrl = QueueUrl,
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
                QueueUrl = QueueUrl,
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
                QueueUrl = QueueUrl,
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
                QueueUrl = QueueUrl,
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
                QueueUrl = QueueUrl,
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
                QueueUrl = QueueUrl,
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
            int MAX_SIZE_KB = 256 * 1024; //Max size allowed by Amazon SQS = 256kb

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

                await _client.CreateQueueAsync(QueueName);
                var queueUrlResponse = await _client.GetQueueUrlAsync(QueueName);
                QueueUrl = queueUrlResponse.QueueUrl;
            }

            return _client;
        }
    }
}