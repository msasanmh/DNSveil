using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace MsmhToolsClass;

// Symmetric Encryption
public static class CypherAES
{
    public static async Task<(bool IsSuccess, byte[] EncryptedBytes, string EncryptedHex)> TryEncryptAsync(byte[] input, string password)
    {
        try
        {
            const int saltSize = 32;
            const int keySize = 256;
            const int key = keySize / 8; // 256 Bits Is Max (/8 To Bytes)
            const int iv = keySize / 16; // AES Needs A 16-Byte IV
            const int iterations = 5000; // Number Of PBKDF2 Iterations (1000 - >10000)

            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
            using Rfc2898DeriveBytes pdb = new(password, salt, iterations);
            byte[] keyBytes = pdb.GetBytes(key);
            byte[] ivBytes = pdb.GetBytes(iv);
            
            using Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.ECB;
            aes.KeySize = keySize;
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
            await cs.WriteAsync(input);
            await cs.FlushFinalBlockAsync();

            byte[] encryptedBytes = salt.Concat(ms.ToArray()).ToArray();
            string encryptedHex = Convert.ToHexString(encryptedBytes); // 1 GB Max
            aes.Clear();
            return (true, encryptedBytes, encryptedHex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CypherAES TryEncryptAsync 1:" + ex.Message);
            return (false, Array.Empty<byte>(), string.Empty);
        }
    }

    public static async Task<(bool IsSuccess, byte[] EncryptedBytes, string EncryptedHex)> TryEncryptAsync(string inputStr, string password)
    {
        try
        {
            byte[] input = Encoding.UTF8.GetBytes(inputStr);
            return await TryEncryptAsync(input, password);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CypherAES TryEncryptAsync 2:" + ex.Message);
            return (false, Array.Empty<byte>(), string.Empty);
        }
    }

    public async static Task<(bool IsSuccess, byte[] DecryptedBytes)> TryDecryptAsync(byte[] encryptedBytes, string password)
    {
        try
        {
            const int saltSize = 32;
            const int keySize = 256;
            const int key = keySize / 8; // 256 Bits Is Max (/8 To Bytes)
            const int iv = keySize / 16; // AES Needs A 16-Byte IV
            const int iterations = 5000; // Number Of PBKDF2 Iterations (1000 - >10000)

            byte[] salt = encryptedBytes.Take(saltSize).ToArray();
            byte[] cypherText = encryptedBytes.Skip(saltSize).ToArray();
            using Rfc2898DeriveBytes pdb = new(password, salt, iterations);
            byte[] keyBytes = pdb.GetBytes(key);
            byte[] ivBytes = pdb.GetBytes(iv);

            using Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.ECB;
            aes.KeySize = keySize;
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Write);
            await cs.WriteAsync(cypherText);
            await cs.FlushFinalBlockAsync();

            byte[] decryptedBytes = ms.ToArray();
            aes.Clear();
            return (true, decryptedBytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CypherAES TryDecryptAsync 1:" + ex.Message);
            return (false, Array.Empty<byte>());
        }
    }

    public async static Task<(bool IsSuccess, byte[] DecryptedBytes)> TryDecryptAsync(string encryptedHex, string password)
    {
        try
        {
            byte[] encryptedBytes = Convert.FromHexString(encryptedHex);
            return await TryDecryptAsync(encryptedBytes, password);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CypherAES TryDecryptAsync 2:" + ex.Message);
            return (false, Array.Empty<byte>());
        }
    }

    public  static (bool IsSuccess, string DecryptedText) TryConvertToString(byte[] decryptedBytes)
    {
        try
        {
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);
            return (true, decryptedText);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CypherAES TryConvertToString:" + ex.Message);
            return (false, string.Empty);
        }
    }
}