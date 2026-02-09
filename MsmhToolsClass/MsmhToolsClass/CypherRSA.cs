using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace MsmhToolsClass;

// Asymmetric Encryption
// Max Lengh Support 379 Characters
public static class CypherRSA
{
    public static void GenerateKeys(out byte[] publicKey, out byte[] privateKey)
    {
		try
		{
			using RSA rsaKey = RSA.Create(4096);
			publicKey = rsaKey.ExportRSAPublicKey();
			privateKey = rsaKey.ExportRSAPrivateKey();
			rsaKey.Clear();
		}
		catch (Exception ex)
		{
			publicKey = Array.Empty<byte>();
            privateKey = Array.Empty<byte>();
			Debug.WriteLine("CypherTool GenerateKeys: " + ex.Message);
        }
    }

	public static bool TryEncrypt(byte[] input, byte[] publicKey, out byte[] encryptedBytes, out string encryptedHex)
	{
		try
		{
			using RSA rsaKey = RSA.Create(4096);
			rsaKey.ImportRSAPublicKey(publicKey, out _);
			encryptedBytes = rsaKey.Encrypt(input, RSAEncryptionPadding.OaepSHA512);
            encryptedHex = Convert.ToHexString(encryptedBytes);

            rsaKey.Clear();
			return true;
		}
		catch (Exception ex)
		{
            encryptedBytes = Array.Empty<byte>();
            encryptedHex = string.Empty;
            Debug.WriteLine("CypherTool TryEncrypt 1: " + ex.Message);
            return false;
        }
    }

    public static bool TryEncrypt(string inputText, byte[] publicKey, out byte[] encryptedBytes, out string encryptedHex)
	{
		try
		{
			byte[] inputBytes = Encoding.UTF8.GetBytes(inputText);
			return TryEncrypt(inputBytes, publicKey, out encryptedBytes, out encryptedHex);
		}
		catch (Exception ex)
		{
            encryptedBytes = Array.Empty<byte>();
            encryptedHex = string.Empty;
            Debug.WriteLine("CypherTool TryEncrypt 2: " + ex.Message);
            return false;
        }
    }

	public static bool TryDecrypt(byte[] encryptedBytes, byte[] privateKey, out byte[] decryptedBytes)
	{
		try
		{
			using RSA rsaKey = RSA.Create(4096);
			rsaKey.ImportRSAPrivateKey(privateKey, out _);
			decryptedBytes = rsaKey.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA512);
            rsaKey.Clear();
			return true;
        }
		catch (Exception ex)
		{
            decryptedBytes = Array.Empty<byte>();
            Debug.WriteLine("CypherTool TryDecrypt 1: " + ex.Message);
            return false;
        }
    }

    public static bool TryDecrypt(string encryptedHex, byte[] privateKey, out byte[] decryptedBytes)
	{
		try
		{
			byte[] encryptedBytes = Convert.FromHexString(encryptedHex);
			return TryDecrypt(encryptedBytes, privateKey, out decryptedBytes);
		}
		catch (Exception ex)
		{
            decryptedBytes = Array.Empty<byte>();
            Debug.WriteLine("CypherTool TryDecrypt 2: " + ex.Message);
            return false;
        }
    }

    public static bool TryDecrypt(byte[] encryptedBytes, byte[] privateKey, out string decryptedText)
	{
		bool isDecryptionSuccess = TryDecrypt(encryptedBytes, privateKey, out byte[] decryptedBytes);
		if (isDecryptionSuccess)
		{
			try
			{
				decryptedText = Encoding.UTF8.GetString(decryptedBytes);
				return true;
			}
			catch (Exception ex)
			{
                decryptedText = string.Empty;
                Debug.WriteLine("CypherTool TryDecrypt 3: " + ex.Message);
                return false;
            }
        }
		else
		{
            decryptedText = string.Empty;
			return false;
        }
	}

    public static bool TryDecrypt(string encryptedHex, byte[] privateKey, out string decryptedText)
	{
		try
		{
			byte[] encryptedBytes = Convert.FromHexString(encryptedHex);
			return TryDecrypt(encryptedBytes, privateKey, out decryptedText);
		}
		catch (Exception ex)
		{
            decryptedText = string.Empty;
            Debug.WriteLine("CypherTool TryDecrypt 4: " + ex.Message);
            return false;
        }
    }
}