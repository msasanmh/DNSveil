using System.Runtime.InteropServices;

namespace MsmhToolsClass.ExternLibs;

#pragma warning disable CA1401 // P/Invokes should not be visible
internal class LibSodium
{
    private const string DllName = "libsodium";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_core_hchacha20(byte[] Hash, byte[] Data, byte[] Key, byte[] Nonce);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_curve25519xchacha20poly1305_beforenm(byte[] SharedKey, byte[] PublicKey, byte[] SecretKey);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_curve25519xchacha20poly1305_easy_afternm(byte[] CipherText, byte[] Message, long MessageLength, byte[] Nonce, byte[] Key);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_curve25519xchacha20poly1305_easy(byte[] CipherText, byte[] Message, long MessageLength, byte[] Nonce, byte[] PublicKey, byte[] SecretKey);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_curve25519xchacha20poly1305_open_easy_afternm(byte[] PlainText, byte[] CipherText, long CipherTextLength, byte[] Nonce, byte[] Key);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_curve25519xchacha20poly1305_open_easy(byte[] PlainText, byte[] CipherText, long CipherTextLength, byte[] Nonce, byte[] PublicKey, byte[] SecretKey);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_easy_afternm(byte[] CipherText, byte[] Message, long MessageLength, byte[] Nonce, byte[] Key);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_easy(byte[] CipherText, byte[] Message, long MessageLength, byte[] Nonce, byte[] PublicKey, byte[] SecretKey);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_open_easy_afternm(byte[] PlainText, byte[] CipherText, long CipherTextLength, byte[] Nonce, byte[] Key);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_open_easy(byte[] PlainText, byte[] CipherText, long CipherTextLength, byte[] Nonce, byte[] PublicKey, byte[] SecretKey);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_beforenm(byte[] SharedKey, byte[] PublicKey, byte[] SecretKey);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void sodium_init();

    //randombytes_buf
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void randombytes_buf(byte[] Buffer, int Size);

    //randombytes_uniform
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int randombytes_uniform(int UpperBound);

    //sodium_increment
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void sodium_increment(byte[] Buffer, long Length);

    //sodium_compare
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int sodium_compare(byte[] A, byte[] B, long Length);

    //sodium_version_string
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr sodium_version_string();

    //crypto_hash
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_hash(byte[] Buffer, byte[] Message, long Length);

    //crypto_hash_sha512
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_hash_sha512(byte[] Buffer, byte[] Message, long Length);

    //crypto_hash_sha256
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_hash_sha256(byte[] Buffer, byte[] Message, long Length);

    //crypto_generichash
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_generichash(byte[] Buffer, int BufferLength, byte[] Message, long MessageLength, byte[] Key, int KeyLength);

    //crypto_generichash_blake2b_salt_personal
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_generichash_blake2b_salt_personal(byte[] Buffer, int BufferLength, byte[] Message, long MessageLength, byte[] Key, int KeyLength, byte[] Salt, byte[] Personal);

    //crypto_onetimeauth
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_onetimeauth(byte[] Buffer, byte[] Message, long MessageLength, byte[] Key);

    //crypto_onetimeauth_verify
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_onetimeauth_verify(byte[] Signature, byte[] Message, long MessageLength, byte[] Key);

    //crypto_pwhash_str
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_pwhash_str(byte[] Buffer, byte[] Password, long PasswordLen, long OpsLimit, int MemLimit);

    //crypto_pwhash_str_verify
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_pwhash_str_verify(byte[] Buffer, byte[] Password, long PassLength);

    //crypto_pwhash
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_pwhash(byte[] Buffer, long BufferLen, byte[] Password, long PasswordLen, byte[] Salt, long OpsLimit, int MemLimit, int Alg);

    //crypto_pwhash_scryptsalsa208sha256_str
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_pwhash_scryptsalsa208sha256_str(byte[] Buffer, byte[] Password, long PasswordLen, long OpsLimit, int MemLimit);

    //crypto_pwhash_scryptsalsa208sha256
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_pwhash_scryptsalsa208sha256(byte[] Buffer, long BufferLen, byte[] Password, long PasswordLen, byte[] Salt, long OpsLimit, int MemLimit);

    //crypto_pwhash_scryptsalsa208sha256_str_verify
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_pwhash_scryptsalsa208sha256_str_verify(byte[] Buffer, byte[] Password, long PassLength);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_pwhash_str_needs_rehash(byte[] Buffer, long OpsLimit, int MemLimit);

    //crypto_sign_keypair
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_keypair(byte[] PublicKey, byte[] SecretKey);

    //crypto_sign_seed_keypair
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_seed_keypair(byte[] PublicKey, byte[] SecretKey, byte[] Seed);

    //crypto_sign
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign(byte[] Buffer, ref long BufferLength, byte[] Message, long MessageLength, byte[] Key);

    //crypto_sign_open
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_open(byte[] Buffer, ref long BufferLength, byte[] SignedMessage, long SignedMessageLength, byte[] Key);

    //crypto_sign_detached
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_detached(byte[] Signature, ref long SignatureLength, byte[] Message, long MessageLength, byte[] Key);

    //crypto_sign_verify_detached
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_verify_detached(byte[] Signature, byte[] Message, long MessageLength, byte[] Key);

    //crypto_sign_ed25519_sk_to_seed
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_ed25519_sk_to_seed(byte[] Seed, byte[] SecretKey);

    //crypto_sign_ed25519_sk_to_pk
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_ed25519_sk_to_pk(byte[] PublicKey, byte[] SecretKey);

    //crypto_sign_ed25519_pk_to_curve25519
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_ed25519_pk_to_curve25519(byte[] Curve25519Pk, byte[] Ed25519Pk);

    //crypto_sign_ed25519_sk_to_curve25519
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_sign_ed25519_sk_to_curve25519(byte[] Curve25519Sk, byte[] Ed25519Sk);

    //crypto_box_keypair
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_keypair(byte[] PublicKey, byte[] SecretKey);

    //crypto_box_seed_keypair
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_seed_keypair(byte[] PublicKey, byte[] SecretKey, byte[] Seed);

    //crypto_box_open_easy
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_detached(byte[] Cipher, byte[] Mac, byte[] Message, long MessageLength, byte[] Nonce, byte[] Pk, byte[] Sk);

    //crypto_box_open_detached
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_open_detached(byte[] Buffer, byte[] CipherText, byte[] Mac, long CipherTextLength, byte[] Nonce, byte[] Pk, byte[] Sk);

    //crypto_box_seedbytes
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_seedbytes();

    //crypto_scalarmult_bytes
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_scalarmult_bytes();

    //crypto_scalarmult_scalarbytes
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_scalarmult_scalarbytes();

    //crypto_scalarmult_primitive
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern byte crypto_scalarmult_primitive();

    //crypto_scalarmult_base
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_scalarmult_base(byte[] PublicKey, byte[] SecretKey);

    //crypto_scalarmult
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_scalarmult(byte[] SharedKey, byte[] SecretKey, byte[] PublicKey);

    //crypto_box_seal
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_seal(byte[] Buffer, byte[] Message, long MessageLength, byte[] Pk);

    //crypto_box_seal_open
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_box_seal_open(byte[] Buffer, byte[] CipherText, long CipherTextLength, byte[] Pk, byte[] Sk);

    //crypto_secretbox_easy
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_secretbox_easy(byte[] Buffer, byte[] Message, long MessageLength, byte[] Nonce, byte[] Key);

    //crypto_secretbox_open_easy
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_secretbox_open_easy(byte[] Buffer, byte[] CipherText, long CipherTextLength, byte[] Nonce, byte[] Key);

    //crypto_secretbox_detached
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_secretbox_detached(byte[] Cipher, byte[] Mac, byte[] Message, long MessageLength, byte[] Nonce, byte[] Key);

    //crypto_secretbox_open_detached
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_secretbox_open_detached(byte[] Buffer, byte[] CipherText, byte[] Mac, long CipherTextLength, byte[] Nonce, byte[] Key);

    //crypto_auth
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_auth(byte[] Buffer, byte[] Message, long MessageLength, byte[] Key);

    //crypto_auth_verify
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_auth_verify(byte[] Signature, byte[] Message, long MessageLength, byte[] Key);

    //crypto_auth_hmacsha256
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_auth_hmacsha256(byte[] Buffer, byte[] Message, long MessageLength, byte[] Key);

    //crypto_auth_hmacsha256_verify
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_auth_hmacsha256_verify(byte[] Signature, byte[] Message, long MessageLength, byte[] Key);

    //crypto_auth_hmacsha512
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_auth_hmacsha512(byte[] Signature, byte[] Message, long MessageLength, byte[] Key);

    //crypto_auth_hmacsha512_verify
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_auth_hmacsha512_verify(byte[] Signature, byte[] Message, long MessageLength, byte[] Key);

    //crypto_shorthash
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_shorthash(byte[] Buffer, byte[] Message, long MessageLength, byte[] Key);

    //crypto_stream_xor
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_stream_xor(byte[] Buffer, byte[] Message, long MessageLength, byte[] Nonce, byte[] Key);

    //crypto_stream_chacha20_xor
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_stream_chacha20_xor(byte[] Buffer, byte[] Message, long MessageLength, byte[] Nonce, byte[] Key);

    //sodium_bin2hex
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr sodium_bin2hex(byte[] Hex, int HexMaxlen, byte[] Bin, int BinLen);

    //sodium_hex2bin
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int sodium_hex2bin(IntPtr Bin, int BinMaxlen, string Hex, int HexLen, string Ignore, out int BinLen, string HexEnd);

    //sodium_bin2base64
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr sodium_bin2base64(byte[] B64, int B64Maxlen, byte[] Bin, int BinLen, int Variant);

    //sodium_base642bin
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int sodium_base642bin(IntPtr Bin, int BinMaxlen, string B64, int B64Len, string Ignore, out int BinLen, out char B64End, int Variant);

    //sodium_base64_encoded_len
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int sodium_base64_encoded_len(int BinLen, int Variant);

    //crypto_aead_chacha20poly1305_ietf_encrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_chacha20poly1305_ietf_encrypt(IntPtr Cipher, out long CipherLength, byte[] Message, long MessageLength, byte[] AdditionalData, long AdditionalDataLength, byte[] NSEC, byte[] Nonce, byte[] Key);

    //crypto_aead_chacha20poly1305_ietf_decrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_chacha20poly1305_ietf_decrypt(IntPtr Message, out long MessageLength, byte[] NSEC, byte[] Cipher, long CipherLength, byte[] AdditionalData, long AdditionalDataLength, byte[] Nonce, byte[] Key);

    //crypto_aead_chacha20poly1305_encrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_chacha20poly1305_encrypt(IntPtr Cipher, out long CipherLength, byte[] Message, long MessageLength, byte[] AdditionalData, long AdditionalDataLength, byte[] NSEC, byte[] Nonce, byte[] Key);

    //crypto_aead_chacha20poly1305_decrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_chacha20poly1305_decrypt(IntPtr Message, out long MessageLength, byte[] NSEC, byte[] Cipher, long CipherLength, byte[] AdditionalData, long AdditionalDataLength, byte[] Nonce, byte[] Key);

    //crypto_aead_xchacha20poly1305_ietf_encrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_xchacha20poly1305_ietf_encrypt(IntPtr Cipher, out long CipherLength, byte[] Message, long MessageLength, byte[] AdditionalData, long AdditionalDataLength, byte[] NSEC, byte[] Nonce, byte[] Key);

    //crypto_aead_xchacha20poly1305_ietf_decrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_xchacha20poly1305_ietf_decrypt(IntPtr Message, out long MessageLength, byte[] NSEC, byte[] Cipher, long CipherLength, byte[] AdditionalData, long AdditionalDataLength, byte[] Nonce, byte[] Key);

    //crypto_aead_aes256gcm_is_available
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_aes256gcm_is_available();

    //crypto_aead_aes256gcm_encrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_aes256gcm_encrypt(IntPtr Cipher, out long CipherLength, byte[] Message, long MessageLength, byte[] AdditionalData, long AdditionalDataLength, byte[] NSEC, byte[] Nonce, byte[] Key);

    //crypto_aead_aes256gcm_decrypt
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_aead_aes256gcm_decrypt(IntPtr Message, out long MessageLength, byte[] NSEC, byte[] Cipher, long CipherLength, byte[] AdditionalData, long AdditionalDataLength, byte[] Nonce, byte[] Key);

    //crypto_generichash_init
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_generichash_init(IntPtr State, byte[] Key, int KeySize, int HashSize);

    //crypto_generichash_update
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_generichash_update(IntPtr State, byte[] Message, long MessageLength);

    //crypto_generichash_final
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_generichash_final(IntPtr State, byte[] Buffer, int BufferLength);

    //crypto_generichash_state
    [StructLayout(LayoutKind.Sequential, Size = 384)]
    internal struct HashState
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] h;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ulong[] t;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ulong[] f;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] buf;
        public uint buflen;
        public byte last_node;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_stream_xchacha20(byte[] Buffer, int BufferLength, byte[] Nonce, byte[] Key);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int crypto_stream_xchacha20_xor(byte[] Buffer, byte[] Message, long MessageLength, byte[] Nonce, byte[] Key);
}
#pragma warning restore CA1401 // P/Invokes should not be visible