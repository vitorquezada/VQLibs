using Azure.Storage.Blobs.Models;

namespace VQLib.Azure.Storage.Blob
{
    public interface IVQAzureStorageService
    {
        Task Delete(
            string key,
            CancellationToken cancellationToken = default);

        Task<MemoryStream?> Get(
            string key,
            MemoryStream? @default = null,
            CancellationToken cancellationToken = default);

        Task<string?> GetKeyByUrl(string url);

        Task<List<BlobItem>> ListByPrefix(
            string prefixKey,
            CancellationToken cancellationToken = default);

        Task<string> Upload(
            Stream data,
            string key,
            string? ContentType = null,
            IDictionary<string, string>? tags = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            int? maxConcurrency = null,
            int bufferSize = 100 * 1024 * 1024);
    }
}