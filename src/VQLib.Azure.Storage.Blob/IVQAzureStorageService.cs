namespace VQLib.Azure.Storage.Blob
{
    public interface IVQAzureStorageService
    {
        Task<MemoryStream> Get(string key, MemoryStream? @default = null);

        Task<string> Upload(Stream data, string key, string? ContentType = null, IDictionary<string, string>? tags = null);

        Task Delete(string key);

        Task<string?> GetKeyByUrl(string url);
    }
}