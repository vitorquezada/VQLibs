namespace VQLib.Azure.Storage.Blob
{
    public interface IVQAzureStorageService
    {
        Task<MemoryStream> Get(string key, MemoryStream? @default = null);

        Task<string> Upload(Stream data, string key, string? ContentType = null);

        Task Delete(string key);
    }
}