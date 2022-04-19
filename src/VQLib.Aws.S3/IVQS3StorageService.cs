namespace VQLib.Aws.S3
{
    public interface IVQS3StorageService
    {
        Task<MemoryStream> Get(string key, MemoryStream? @default = null);

        Task<string> Upload(Stream data, string key, bool isPublic = true, string? ContentType = null);

        Task Delete(string key);
    }
}