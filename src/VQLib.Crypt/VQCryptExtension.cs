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

        public static string GetMd5Hash(this string text) => GetMd5Hash(StringToUtf8Byte(text));

        public static string GetMd5Hash(this byte[] bytes)
        {
            using var hashAlgorithm = new MD5CryptoServiceProvider();
            return GetHash(bytes, hashAlgorithm);
        }

        public static string GetSha256Hash(this string text) => GetSha256Hash(StringToUtf8Byte(text));

        public static string GetSha256Hash(this byte[] bytes)
        {
            using var hashAlgorithm = new SHA256Managed();
            return GetHash(bytes, hashAlgorithm);
        }

        public static string GetHash<T>(this byte[] bytes, T hashAlgorithm) where T : HashAlgorithm, new()
        {
            hashAlgorithm.ComputeHash(bytes);
            return BitConverter.ToString(hashAlgorithm.Hash).Replace(BYTE_SEPARATOR, STRING_EMPTY);
        }

        public static string EncryptAes(this string text, string key, string iv)
            => Convert.ToBase64String(Encrypt<AesCryptoServiceProvider>(text.StringToUtf8Byte(), key.StringToUtf8Byte(), iv.StringToUtf8Byte()));

        public static string DecryptAes(this string crypt, string key, string iv)
            => Decrypt<AesCryptoServiceProvider>(Convert.FromBase64String(crypt), key.StringToUtf8Byte(), iv.StringToUtf8Byte()).Utf8ByteToString();

        public static byte[] Encrypt<T>(this byte[] bytes, byte[] key, byte[] iv) where T : SymmetricAlgorithm, new()
        {
            using var alg = new T();
            alg.Key = key;
            alg.IV = iv;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(bytes, 0, bytes.Length);
            return ms.ToArray();
        }

        public static byte[] Decrypt<T>(this byte[] bytes, byte[] key, byte[] iv) where T : SymmetricAlgorithm, new()
        {
            using var alg = new T();
            alg.Key = key;
            alg.IV = iv;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Read);
            cs.Read(bytes, 0, bytes.Length);
            return ms.ToArray();
        }

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