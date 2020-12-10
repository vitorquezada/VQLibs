using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VQLibs.Util.Crypt.Enum;
using VQLibs.Util.Crypt.External;

namespace VQLibs.Util.Crypt
{
    public static class VQCryptExtension
    {
        private const char BYTE_SEPARATOR = '-';
        private const char CHAR_EMPTY = '\0';

        public static byte[] StringToUtf8Byte(string text) => Encoding.UTF8.GetBytes(text);
        public static byte[] StringToAsciiByte(string text) => Encoding.ASCII.GetBytes(text);

        public static string GetMd5Hash(this string text) => GetMd5Hash(StringToUtf8Byte(text));
        public static string GetMd5Hash(this byte[] bytes)
        {
            using (var hash = MD5.Create())
                return GetHash(bytes, hash);
        }

        public static string GetSha256Hash(this string text) => GetMd5Hash(StringToUtf8Byte(text));
        public static string GetSha256Hash(this byte[] bytes)
        {
            using (var hash = HMACSHA256.Create())
                return GetHash(bytes, hash);
        }

        public static string GetHashArgon(
            string text,
            ulong opsLimite = SodiumLibrary.crypto_pwhash_argon2id_OPSLIMIT_MODERATE,
            int memLimit = SodiumLibrary.crypto_pwhash_argon2id_MEMLIMIT_MODERATE)
        {
            var buffer = new byte[128];

            var texthash = StringToUtf8Byte(text);

            var result = SodiumLibrary.crypto_pwhash_str(
                buffer,
                texthash,
                Convert.ToUInt64(texthash.GetLongLength(0)),
                Convert.ToUInt64(opsLimite),
                memLimit);

            if (result != 0)
                throw new OutOfMemoryException("Out of memory in get hash argon2");

            var bufferTrim = buffer.Where(x => x != 0x00).ToArray();

            return BitConverter.ToString(bufferTrim).Replace(BYTE_SEPARATOR, CHAR_EMPTY);
        }

        public static VQEnumVerifyHashOut VerifyHashArgon(
            string hash,
            string password,
            ulong opsLimite = SodiumLibrary.crypto_pwhash_argon2id_OPSLIMIT_MODERATE,
            int memLimit = SodiumLibrary.crypto_pwhash_argon2id_MEMLIMIT_MODERATE)
        {
            var passwdHash = StringToUtf8Byte(password);
            var hashBytes = HexStringToByte(hash);
            var ret = SodiumLibrary.crypto_pwhash_str_verify(
                hashBytes,
                passwdHash,
                Convert.ToUInt64(passwdHash.GetLongLength(0)));

            if (ret != 0)
            {
                return VQEnumVerifyHashOut.FAILED;
            }

            ret = SodiumLibrary.crypto_pwhash_str_needs_rehash(hashBytes, opsLimite, memLimit);
            if (ret < 0)
            {
                throw new Exception($"crypto_pwhash_str_needs_rehash: failed with result {ret}");
            }

            return ret == 0 ? VQEnumVerifyHashOut.PASSED : VQEnumVerifyHashOut.NEEDS_REHASH;
        }

        private static string GetHash(byte[] bytes, HashAlgorithm hashAlgorithm)
        {
            if (hashAlgorithm.HashSize > 0)
                hashAlgorithm.Clear();
            hashAlgorithm.ComputeHash(bytes);
            return BitConverter.ToString(hashAlgorithm.Hash).Replace(BYTE_SEPARATOR, CHAR_EMPTY);
        }

        private static byte[] HexStringToByte(string hexString)
        {
            var hashBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
                hashBytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return hashBytes;
        }
    }
}
