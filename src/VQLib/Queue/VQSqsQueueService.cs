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
    public class VQSqsQueueService<T> : IVQQueueService<T>
    {
        private readonly VQAwsConfigModel _configModel;

        public VQSqsQueueService(VQAwsConfigModel configModel)
        {
            _configModel = configModel;
        }

        public async Task ChangeVisibilityManyTimeout(IEnumerable<string> receipt, string queueName, int visibilityTimeoutSeconds = 3600)
        {
            var obj = new ChangeMessageVisibilityBatchRequest
            {
                Entries = receipt.Select((x, i) => new ChangeMessageVisibilityBatchRequestEntry
                {
                    Id = i.ToString(),
                    ReceiptHandle = x,
                    VisibilityTimeout = visibilityTimeoutSeconds
                }).ToList(),
                QueueUrl = GetQueueUrl(queueName),
            };

            using var client = GetClient(queueName);
            var response = await client.ChangeMessageVisibilityBatchAsync(obj);
        }

        public async Task ChangeVisibilityTimeout(string receipt, string queueName, int visibilityTimeoutSeconds = 3600)
        {
            var obj = new ChangeMessageVisibilityRequest
            {
                ReceiptHandle = receipt,
                QueueUrl = GetQueueUrl(queueName),
                VisibilityTimeout = visibilityTimeoutSeconds,
            };

            using var client = GetClient(queueName);
            var response = await client.ChangeMessageVisibilityAsync(obj);
        }

        public async Task Confirm(QueueResponse response, string queueName)
        {
            await Confirm(response.Receipt, queueName);
        }

        public async Task Confirm(string receipt, string queueName)
        {
            var delete = new DeleteMessageRequest
            {
                ReceiptHandle = receipt,
                QueueUrl = GetQueueUrl(queueName),
            };

            using var client = GetClient(queueName);
            await client.DeleteMessageAsync(delete);
        }

        public async Task ConfirmMany(IEnumerable<QueueResponse> response, string queueName)
        {
            await ConfirmMany(response.Select(x => x.Receipt).ToHashSet(), queueName);
        }

        public async Task ConfirmMany(IEnumerable<string> receipt, string queueName)
        {
            var delete = new DeleteMessageBatchRequest
            {
                Entries = receipt.Select((x, i) => new DeleteMessageBatchRequestEntry
                {
                    ReceiptHandle = x,
                    Id = i.ToString(),
                }).ToList(),
                QueueUrl = GetQueueUrl(queueName),
            };

            using var client = GetClient(queueName);
            await client.DeleteMessageBatchAsync(delete);
        }

        public async Task<List<T>> Dequeue(string queueName)
        {
            var messageResponses = await GetMessages(queueName);

            var response = new List<T>();
            foreach (var message in messageResponses.Messages)
            {
                response.Add(message.Body.FromJson<T>());
                await Confirm(message.ReceiptHandle, queueName);
            }

            return response;
        }

        public async Task<List<QueueResponse<T>>> DequeueConfirmation(string queueName, int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null)
        {
            var messageResponses = await GetMessages(queueName, visibilityTimeoutSeconds, maxNumberOfMessages, waitTimeSeconds);

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

        public async Task Enqueue(T data, string queueName, int delayInSeconds = 0)
        {
            if (delayInSeconds > 900)
                throw new ArgumentOutOfRangeException("Impossible to enqueue item. The maximum value for delay is 900 (=15minutes)");

            var dataSerialized = data.ToJson();
            CheckDataSize(dataSerialized);

            var message = new SendMessageRequest
            {
                QueueUrl = GetQueueUrl(queueName),
                DelaySeconds = delayInSeconds,
                MessageBody = dataSerialized
            };

            using var client = GetClient(queueName);
            var response = await client.SendMessageAsync(message);
        }

        public async Task Enqueue(List<T> dataList, string queueName, int delayInSeconds = 0)
        {
            var request = new SendMessageBatchRequest
            {
                QueueUrl = GetQueueUrl(queueName),
                Entries = dataList.Select((x, index) => new SendMessageBatchRequestEntry
                {
                    Id = index.ToString(),
                    DelaySeconds = delayInSeconds,
                    MessageBody = x.ToJson(),
                }).ToList()
            };

            using var client = GetClient(queueName);
            var response = await client.SendMessageBatchAsync(request);
        }

        private string GetQueueUrl(string queueName)
        {
            return $"{_configModel.Region.GetEndpointForService("sqs")}/{_configModel.AccountNumber}/{queueName}";
        }

        private IAmazonSQS GetClient(string queueName)
        {
            return _configModel.AccessKey.IsNotNullOrWhiteSpace() && _configModel.SecretKey.IsNotNullOrWhiteSpace()
                ? new AmazonSQSClient(_configModel.AccessKey, _configModel.SecretKey, _configModel.Region)
                : new AmazonSQSClient(_configModel.Region);
        }

        private async Task<ReceiveMessageResponse> GetMessages(string queueName, int visibilityTimeoutSeconds = 3600, int maxNumberOfMessages = 10, int? waitTimeSeconds = null)
        {
            var messageRequest = new ReceiveMessageRequest()
            {
                QueueUrl = GetQueueUrl(queueName),
                MaxNumberOfMessages = maxNumberOfMessages,
                VisibilityTimeout = visibilityTimeoutSeconds,
            };

            if (waitTimeSeconds.GetValueOrDefault() > 0)
                messageRequest.WaitTimeSeconds = waitTimeSeconds.Value;

            using var client = GetClient(queueName);
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
    }
}