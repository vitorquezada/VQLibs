using System.Security.Cryptography;
using System.Text;
using VQLib.Crypt.Enum;
using VQLib.Crypt.External;

namespace VQLib.Crypt
{
    public static class VQCryptExtension
    {
        private const string BYTE_SEPARATOR = "-";
        private const string STRING_EMPTY = "";

        public static byte[] StringToUtf8Byte(this string text) => text != null ? Encoding.UTF8.GetBytes(text) : Array.Empty<byte>();

        public static string Utf8ByteToString(this byte[] bytes) => bytes != null ? Encoding.UTF8.GetString(bytes) : string.Empty;

        public static byte[] StringToAsciiByte(this string text) => text != null ? Encoding.ASCII.GetBytes(text) : Array.Empty<byte>();

        public static string AsciiByteToString(this byte[] bytes) => bytes != null ? Encoding.ASCII.GetString(bytes) : string.Empty;

        #region MD5

        public static string GetMd5Hash(this string text) => GetMd5Hash(StringToUtf8Byte(text));

        public static async Task<string> GetMd5HashAsync(this string text) => await GetMd5HashAsync(StringToUtf8Byte(text));

        public static string GetMd5Hash(this byte[] bytes)
        {
            using var hashAlgorithm = MD5.Create();
            return GetHash(bytes, hashAlgorithm);
        }

        public static async Task<string> GetMd5HashAsync(this byte[] bytes)
        {
            using var hashAlgorithm = MD5.Create();
            return await GetHashAsync(bytes, hashAlgorithm);
        }

        #endregion MD5

        #region SHA256

        public static string GetSha256Hash(this string text) => GetSha256Hash(StringToUtf8Byte(text));

        public static async Task<string> GetSha256HashAsync(this string text) => await GetSha256HashAsync(StringToUtf8Byte(text));

        public static string GetSha256Hash(this byte[] bytes) => GetSha256HashAsync(bytes).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<string> GetSha256HashAsync(this byte[] bytes)
        {
            using var hashAlgorithm = SHA256.Create();
            return await GetHashAsync(bytes, hashAlgorithm);
        }

        #endregion SHA256

        #region HASH

        public static string GetHash<T>(this byte[] bytes, T hashAlgorithm) where T : HashAlgorithm => GetHashAsync(bytes, hashAlgorithm).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<string> GetHashAsync<T>(this byte[] bytes, T hashAlgorithm) where T : HashAlgorithm
        {
            using var ms = new MemoryStream(bytes);
            await hashAlgorithm.ComputeHashAsync(ms);
            ArgumentNullException.ThrowIfNull(hashAlgorithm.Hash);
            return BitConverter.ToString(hashAlgorithm.Hash).Replace(BYTE_SEPARATOR, STRING_EMPTY);
        }

        #endregion HASH

        #region AES

        public static string EncryptAes(this string text, string key, string iv) => EncryptAes(text, key.StringToUtf8Byte(), iv.StringToUtf8Byte());

        public static async Task<string> EncryptAesAsync(this string text, string key, string iv) => await EncryptAesAsync(text, key.StringToUtf8Byte(), iv.StringToUtf8Byte());

        public static string EncryptAes(this string text, byte[] key, byte[] iv) => EncryptAesAsync(text, key, iv).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<string> EncryptAesAsync(this string text, byte[] key, byte[] iv)
        {
            using var alg = Aes.Create();
            alg.Key = key;
            alg.IV = iv;
            alg.Mode = CipherMode.CBC;
            alg.Padding = PaddingMode.PKCS7;
            return Convert.ToBase64String(await EncryptAsync(text.StringToUtf8Byte(), alg));
        }

        public static string DecryptAes(this string crypt, string key, string iv) => DecryptAes(crypt, key.StringToUtf8Byte(), iv.StringToUtf8Byte());

        public static async Task<string> DecryptAesAsync(this string crypt, string key, string iv) => await DecryptAesAsync(crypt, key.StringToUtf8Byte(), iv.StringToUtf8Byte());

        public static string DecryptAes(this string crypt, byte[] key, byte[] iv) => DecryptAesAsync(crypt, key, iv).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<string> DecryptAesAsync(this string crypt, byte[] key, byte[] iv)
        {
            using var alg = Aes.Create();
            alg.Key = key;
            alg.IV = iv;
            alg.Mode = CipherMode.CBC;
            alg.Padding = PaddingMode.PKCS7;
            return (await DecryptAsync(Convert.FromBase64String(crypt), alg)).Utf8ByteToString();
        }

        #endregion AES

        #region SYMMETRIC_ALG

        public static string Encrypt(this string text, SymmetricAlgorithm alg) => Convert.ToBase64String(Encrypt(text.StringToUtf8Byte(), alg));

        public static async Task<string> EncryptAsync(this string text, SymmetricAlgorithm alg) => Convert.ToBase64String(await EncryptAsync(text.StringToUtf8Byte(), alg));

        public static byte[] Encrypt(this byte[] bytes, SymmetricAlgorithm alg) => EncryptAsync(bytes, alg).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<byte[]> EncryptAsync(this byte[] bytes, SymmetricAlgorithm alg)
        {
            using var ms = new MemoryStream();

            using (var enc = alg.CreateEncryptor())
            using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(bytes, 0, bytes.Length);
            }

            return ms.ToArray();
        }

        public static string Decrypt(this string text, SymmetricAlgorithm alg) => Decrypt(Convert.FromBase64String(text), alg).Utf8ByteToString();

        public static async Task<string> DecryptAsync(this string text, SymmetricAlgorithm alg) => (await DecryptAsync(Convert.FromBase64String(text), alg)).Utf8ByteToString();

        public static byte[] Decrypt(this byte[] bytes, SymmetricAlgorithm alg) => DecryptAsync(bytes, alg).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<byte[]> DecryptAsync(this byte[] bytes, SymmetricAlgorithm alg)
        {
            using var msDecrypted = new MemoryStream();

            using (var msEncrypted = new MemoryStream(bytes))
            using (var enc = alg.CreateDecryptor())
            using (var cs = new CryptoStream(msEncrypted, enc, CryptoStreamMode.Read))
            {
                await cs.CopyToAsync(msDecrypted);
            }

            return msDecrypted.ToArray();
        }

        #endregion SYMMETRIC_ALG

        public static string GetHashArgon(
            this string text,
            ulong opsLimite = VQSodiumLibrary.crypto_pwhash_argon2id_OPSLIMIT_MODERATE,
            int memLimit = VQSodiumLibrary.crypto_pwhash_argon2id_MEMLIMIT_MODERATE)
        {
            var buffer = new byte[128];

            var texthash = StringToUtf8Byte(text);

            var result = VQSodiumLibrary.crypto_pwhash_str(
                buffer,
                texthash,
                Convert.ToUInt64(texthash.GetLongLength(0)),
                Convert.ToUInt64(opsLimite),
                memLimit);

            if (result != 0)
                throw new OutOfMemoryException("Out of memory in get hash argon2");

            var bufferTrim = buffer.Where(x => x != 0x00).ToArray();

            return BitConverter.ToString(bufferTrim).Replace(BYTE_SEPARATOR, STRING_EMPTY);
        }

        public static VQVerifyHashResult VerifyHashArgon(
            this string hash,
            string password,
            ulong opsLimite = VQSodiumLibrary.crypto_pwhash_argon2id_OPSLIMIT_MODERATE,
            int memLimit = VQSodiumLibrary.crypto_pwhash_argon2id_MEMLIMIT_MODERATE)
        {
            var passwdHash = StringToUtf8Byte(password);
            var hashBytes = HexStringToByte(hash);
            var ret = VQSodiumLibrary.crypto_pwhash_str_verify(
                hashBytes,
                passwdHash,
                Convert.ToUInt64(passwdHash.GetLongLength(0)));

            if (ret != 0)
            {
                return VQVerifyHashResult.FAILED;
            }

            ret = VQSodiumLibrary.crypto_pwhash_str_needs_rehash(hashBytes, opsLimite, memLimit);
            if (ret < 0)
            {
                throw new Exception($"crypto_pwhash_str_needs_rehash: failed with result {ret}");
            }

            return ret == 0 ? VQVerifyHashResult.PASSED : VQVerifyHashResult.NEEDS_REHASH;
        }

        private static byte[] HexStringToByte(this string hexString)
        {
            var hashBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
                hashBytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return hashBytes;
        }
    }
}