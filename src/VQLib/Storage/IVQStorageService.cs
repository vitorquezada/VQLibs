using System.IO;
using System.Threading.Tasks;

namespace VQLib.Storage
{
    public interface IVQStorageService
    {
        string BucketName { get; set; }

        Task<MemoryStream> Get(string key, MemoryStream @default = null);

        Task<string> Upload(Stream data, string key, bool isPublic = true, string ContentType = null);

        Task Delete(string key);

        Task EnsureBucketExists();
    }
}