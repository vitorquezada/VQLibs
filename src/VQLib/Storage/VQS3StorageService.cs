using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading.Tasks;
using VQLib.AwsCloud.Model;
using VQLib.Util;

namespace VQLib.Storage
{
    public class VQS3StorageService : IVQStorageService
    {
        private const string S3_URL_FORMAT = "https://s3.amazonaws.com/{0}{1}";

        private readonly VQAwsConfigModel _configModel;

        public VQS3StorageService(VQAwsConfigModel configModel)
        {
            _configModel = configModel;
        }

        private IAmazonS3 GetClient => _configModel.AccessKey.IsNotNullOrWhiteSpace() && _configModel.SecretKey.IsNotNullOrWhiteSpace()
            ? new AmazonS3Client(_configModel.AccessKey, _configModel.SecretKey, _configModel.Region)
            : new AmazonS3Client(_configModel.Region);

        private string _bucketName;

        public string BucketName
        {
            get
            {
                if (_bucketName.IsNullOrWhiteSpace()) throw new ArgumentNullException();
                return _bucketName;
            }
            set
            {
                _bucketName = value.ToLower();
            }
        }

        public async Task<MemoryStream> Get(string key, MemoryStream @default = null)
        {
            using var client = GetClient;
            using var obj = await client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = BucketName,
                Key = key,
            });

            if ((obj?.ResponseStream?.Length ?? 0) <= 0)
                return @default;

            using var data = obj.ResponseStream;
            var memoryStream = new MemoryStream();
            data.CopyTo(memoryStream);
            data.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public async Task<string> Upload(Stream data, string key, bool isPublic = true, string ContentType = null)
        {
            using var client = GetClient;
            using var fileTransfer = new TransferUtility(client);

            var request = new TransferUtilityUploadRequest
            {
                AutoResetStreamPosition = true,
                BucketName = BucketName,
                InputStream = data,
                Key = key,
            };

            if (ContentType.IsNotNullOrWhiteSpace())
                request.ContentType = ContentType;

            if (isPublic)
                request.CannedACL = S3CannedACL.PublicRead;

            await fileTransfer.UploadAsync(request);

            return GetFileUrl(key);
        }

        public async Task Delete(string key)
        {
            using var client = GetClient;
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = key,
            };

            await client.DeleteObjectAsync(deleteObjectRequest);
        }

        public async Task EnsureBucketExists()
        {
            using var client = GetClient;
            if (await client.DoesS3BucketExistAsync(BucketName))
                return;

            var response = await client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = BucketName,
            });

            if (!response.HttpStatusCode.IsSuccessStatusCode())
                throw new ApplicationException("Error on EnsureBucketExists");
        }

        private string GetFileUrl(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var keyNameNormalized = key.StartsWith("/") ? key : $"/{key}";
            return string.Format(S3_URL_FORMAT, BucketName, keyNameNormalized);
        }
    }
}