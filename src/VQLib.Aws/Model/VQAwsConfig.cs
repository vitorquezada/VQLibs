using Amazon;

namespace VQLib.Aws.Model
{
    public class VQAwsConfig
    {
        public string? AccessKey { get; set; }

        public string? SecretKey { get; set; }

        public RegionEndpoint? Region { get; set; }
    }
}