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

        public async Task Delete(string key)
        {
            var (containerName, filePath) = SplitKey(key);

            var container = new BlobContainerClient(_connectionString, containerName);

            var containerExists = await container.ExistsAsync();
            if (!containerExists.Value)
                return;

            var blob = container.GetBlobClient(filePath);

            await blob.DeleteIfExistsAsync();
        }

        public async Task<MemoryStream?> Get(string key, MemoryStream? @default = null)
        {
            var (containerName, filePath) = SplitKey(key);

            var container = new BlobContainerClient(_connectionString, containerName);

            var containerExists = await container.ExistsAsync();
            if (!containerExists.Value)
                return @default;

            var blob = container.GetBlobClient(filePath);

            var blobExists = await blob.ExistsAsync();
            if (!blobExists.Value)
                return @default;

            var stream = new MemoryStream();

            await blob.DownloadToAsync(stream);

            return stream;
        }

        public Task<string?> GetKeyByUrl(string url)
        {
            var uri = new Uri(url);
            var key = uri.LocalPath;
            if (key.StartsWith("/")) key = key.Substring(1);
            return Task.FromResult(key);
        }

        public async Task<string> Upload(Stream data, string key, string? ContentType = null)
        {
            var (containerName, filePath) = SplitKey(key);

            var container = new BlobContainerClient(_connectionString, containerName);

            await container.CreateIfNotExistsAsync(PublicAccessType.None);

            var blob = container.GetBlobClient(filePath);

            await blob.UploadAsync(data);

            if (!string.IsNullOrWhiteSpace(ContentType))
            {
                var httpHeader = new BlobHttpHeaders()
                {
                    ContentType = ContentType,
                };

                await blob.SetHttpHeadersAsync(httpHeader);
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