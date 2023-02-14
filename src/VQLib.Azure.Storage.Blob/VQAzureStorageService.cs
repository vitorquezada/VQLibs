using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace VQLib.Azure.Storage.Blob
{
    public class VQAzureStorageService : IVQAzureStorageService
    {
        private readonly string _connectionString;

        public VQAzureStorageService(VQAzureStorageConfig config)
        {
            _connectionString = config.StorageConnectionString ?? throw new ArgumentException($"Property {config.StorageConnectionString} can not be null or whitespace.", nameof(config));
        }

        public async Task Delete(
            string key,
            CancellationToken cancellationToken = default)
        {
            var (containerName, filePath) = SplitKey(key);

            var container = new BlobContainerClient(_connectionString, containerName);

            var containerExists = await container.ExistsAsync(cancellationToken);
            if (!containerExists.Value)
                return;

            var blob = container.GetBlobClient(filePath);

            await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }

        public async Task<MemoryStream?> Get(
            string key,
            MemoryStream? @default = null,
            CancellationToken cancellationToken = default)
        {
            var (containerName, filePath) = SplitKey(key);

            var container = new BlobContainerClient(_connectionString, containerName);

            var containerExists = await container.ExistsAsync(cancellationToken);
            if (!containerExists.Value)
                return @default;

            var blob = container.GetBlobClient(filePath);

            var blobExists = await blob.ExistsAsync(cancellationToken);
            if (!blobExists.Value)
                return @default;

            var stream = new MemoryStream();

            await blob.DownloadToAsync(stream, cancellationToken);

            return stream;
        }

        public Task<string?> GetKeyByUrl(string url)
        {
            var uri = new Uri(url);
            var key = uri.LocalPath;
            if (key.StartsWith("/")) key = key.Substring(1);
            return Task.FromResult<string?>(key);
        }

        public async Task<List<BlobItem>> ListByPrefix(
            string prefixKey,
            CancellationToken cancellationToken = default)
        {
            var blobList = new List<BlobItem>();

            var (containerName, filePath) = SplitKey(prefixKey);
            var container = new BlobContainerClient(_connectionString, containerName);

            var blobs = container.GetBlobsAsync(prefix: filePath, cancellationToken: cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await blobs.MoveNextAsync())
                    blobList.Add(blobs.Current);
            }
            finally
            {
                await blobs.DisposeAsync();
            }

            return blobList;
        }

        public async Task<string> Upload(
            Stream data,
            string key,
            string? ContentType = null,
            IDictionary<string, string>? tags = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            if (data != null)
            {
                data.Seek(0, SeekOrigin.Begin);
                await data.CopyToAsync(memory, cancellationToken);
                await memory.FlushAsync(cancellationToken);
                data.Seek(0, SeekOrigin.Begin);
                memory.Seek(0, SeekOrigin.Begin);
            }

            var (containerName, filePath) = SplitKey(key);

            var options = new BlobClientOptions();
            if (timeout.HasValue)
                options.Retry.NetworkTimeout = timeout.Value;
            var container = new BlobContainerClient(_connectionString, containerName, options);

            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blob = container.GetBlobClient(filePath);

            var optionsUpload = new BlobUploadOptions
            {
                TransferOptions = new StorageTransferOptions
                {
                    MaximumConcurrency = 2 * Environment.ProcessorCount,
                    InitialTransferSize = 100 * 1024 * 1024,
                    MaximumTransferSize = 100 * 1024 * 1024,
                },
            };
            await blob.UploadAsync(data, optionsUpload, cancellationToken);

            if (!string.IsNullOrWhiteSpace(ContentType))
            {
                var httpHeader = new BlobHttpHeaders()
                {
                    ContentType = ContentType,
                };

                await blob.SetHttpHeadersAsync(httpHeader, cancellationToken: cancellationToken);
            }

            if (tags != null && tags.Any())
            {
                await blob.SetTagsAsync(tags, cancellationToken: cancellationToken);
            }

            return blob.Uri.AbsoluteUri;
        }

        private (string ContainerName, string FilePath) SplitKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !key.Contains('/'))
                throw new Exception($"Error on {nameof(SplitKey)}. Key must not be empty and must have one or more \"/\" separator");

            var splited = key.Split('/');
            var bucketName = splited[0];
            var filePath = string.Join("/", splited.Skip(1));

            return (bucketName, filePath);
        }
    }
}