using Amazon;

namespace VQLib.AwsCloud.Model
{
    public class VQAwsConfigModel
    {
        public RegionEndpoint Region { get; set; }

        public string SecretKey { get; set; }

        public string AccessKey { get; set; }

        public string AccountNumber { get; set; }
    }
}