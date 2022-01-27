using Amazon;

namespace VQLib.Storage
{
    public class VQS3StorageConfigModel
    {
        public RegionEndpoint Region { get; set; }

        public string SecretKey { get; set; }

        public string AccessKey { get; set; }
    }
}