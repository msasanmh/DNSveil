using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace MsmhToolsClass;

public class EncodingTool
{
    public static string GetSHA1(string text)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA1 hash = SHA1.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string GetSHA256(string text)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA256 hash = SHA256.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string GetSHA384(string text)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA384 hash = SHA384.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string GetSHA512(string text)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA512 hash = SHA512.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string Base64Encode(string plainText)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(data);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string Base64Decode(string encodedString)
    {
        try
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string Base64ToBase64Url(string base64)
    {
        try
        {
            return base64.Replace("=", "").Replace("/", "_").Replace("+", "-");
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string Base64UrlToBase64(string base64Url)
    {
        try
        {
            return base64Url.PadRight(base64Url.Length + (4 - base64Url.Length % 4) % 4, '=').Replace("_", "/").Replace("-", "+");
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string UrlEncode(byte[] buffer)
    {
        try
        {
            string base64 = Convert.ToBase64String(buffer);
            return Base64ToBase64Url(base64);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static byte[] UrlDecode(string base64Url)
    {
        string base64 = Base64UrlToBase64(base64Url);

        try
        {
            
            return Convert.FromBase64String(base64);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UrlDecode: " + ex.Message);
            Debug.WriteLine("UrlDecode Base64Url: " + base64Url);
            Debug.WriteLine("UrlDecode Base64: " + base64);
            return Array.Empty<byte>();
        }
    }

}