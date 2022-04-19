using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using VQLib.Aws.Model;
using VQLib.Util;

namespace VQLib.Aws.S3
{
    public class VQS3StorageService : IVQS3StorageService
    {
        private const string S3_URL_FORMAT = "https://s3.amazonaws.com/{0}{1}";

        private readonly VQAwsConfig _config;

        private IAmazonS3 GetClient => _config.AccessKey.IsNotNullOrWhiteSpace() && _config.SecretKey.IsNotNullOrWhiteSpace()
            ? new AmazonS3Client(_config.AccessKey, _config.SecretKey, _config.Region)
            : new AmazonS3Client(_config.Region);

        public VQS3StorageService(VQAwsConfig config)
        {
            _config = config;
        }

        public async Task<MemoryStream?> Get(string key, MemoryStream? @default = null)
        {
            var (bucketName, filePath) = SplitKey(key);

            using var client = GetClient;

            var bucketExists = await client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
                return @default;

            using var obj = await client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucketName,
                Key = filePath,
            });

            if (obj == null || obj.ResponseStream == null || obj.ResponseStream.Length <= 0)
                return @default;

            using var data = obj.ResponseStream;
            var memoryStream = new MemoryStream();
            data.CopyTo(memoryStream);
            data.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public async Task<string> Upload(Stream data, string key, bool isPublic = true, string? ContentType = null)
        {
            var (bucketName, filePath) = SplitKey(key);

            using var client = GetClient;

            await CreateBucketIfNoExists(client, bucketName);

            using var fileTransfer = new TransferUtility(client);

            var request = new TransferUtilityUploadRequest
            {
                AutoResetStreamPosition = true,
                BucketName = bucketName,
                InputStream = data,
                Key = filePath,
            };

            if (isPublic)
                request.CannedACL = S3CannedACL.PublicRead;

            if (!string.IsNullOrWhiteSpace(ContentType))
                request.ContentType = ContentType;

            await fileTransfer.UploadAsync(request);

            return GetFileUrl(key);
        }

        public async Task Delete(string key)
        {
            var (bucketName, filePath) = SplitKey(key);
            using var client = GetClient;

            var bucketExists = await client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
                return;

            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = filePath,
            };

            await client.DeleteObjectAsync(deleteObjectRequest);
        }

        private async Task<IAmazonS3> CreateBucketIfNoExists(IAmazonS3 client, string bucketName)
        {
            if (await client.DoesS3BucketExistAsync(bucketName))
                return client;

            var response = await client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = bucketName,
            });

            if (!response.HttpStatusCode.IsSuccessStatusCode())
                throw new ApplicationException($"Error on {nameof(CreateBucketIfNoExists)}");

            return client;
        }

        private string GetFileUrl(string key)
        {
            var (bucketName, filePath) = SplitKey(key);

            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            var keyNameNormalized = filePath.StartsWith("/") ? key : $"/{filePath}";
            return string.Format(S3_URL_FORMAT, bucketName, keyNameNormalized);
        }

        private (string BucketName, string FilePath) SplitKey(string key)
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