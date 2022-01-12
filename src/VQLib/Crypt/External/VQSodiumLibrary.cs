using System.Runtime.InteropServices;

namespace VQLib.Crypt.External
{
    public class VQSodiumLibrary
    {
        private const string Name = "libsodium";

        public const ulong crypto_pwhash_argon2id_OPSLIMIT_INTERACTIVE = 2;
        public const int crypto_pwhash_argon2id_MEMLIMIT_INTERACTIVE = 67108864;

        public const ulong crypto_pwhash_argon2id_OPSLIMIT_MODERATE = 3;
        public const int crypto_pwhash_argon2id_MEMLIMIT_MODERATE = 268435456;

        public const ulong crypto_pwhash_argon2id_OPSLIMIT_SENSITIVE = 4;
        public const int crypto_pwhash_argon2id_MEMLIMIT_SENSITIVE = 1073741824;

        static VQSodiumLibrary()
        {
            sodium_init();
        }

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sodium_init();

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void randombytes_buf(byte[] buffer, int size);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_pwhash(byte[] buffer, ulong bufferLen, byte[] password, ulong passwordLen, byte[] salt, ulong opsLimit, int memLimit, int alg);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_pwhash_str(byte[] buffer, byte[] password, ulong passwordLen, ulong opsLimit, int memLimit);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_pwhash_str_verify(byte[] buffer, byte[] password, ulong passLength);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_pwhash_str_needs_rehash(byte[] buffer, ulong opslimit, int memlimit);
    }
}