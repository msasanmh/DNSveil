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

    public static string UrlEncode(byte[] arg)
    {
        try
        {
            string s = Convert.ToBase64String(arg);
            return s.Replace("=", "").Replace("/", "_").Replace("+", "-");
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string UrlToBase64(string arg)
    {
        try
        {
            return arg.PadRight(arg.Length + (4 - arg.Length % 4) % 4, '=').Replace("_", "/").Replace("-", "+");
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static byte[] UrlDecode(string arg)
    {
        try
        {
            string decrypted = UrlToBase64(arg);
            return Convert.FromBase64String(decrypted);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UrlDecode: " + ex.Message);
            return Array.Empty<byte>();
        }
    }

    public static T[] SubArray<T>(T[] arr, int start, int length)
    {
        T[] result = new T[length];
        try
        {
            Buffer.BlockCopy(arr, start, result, 0, length);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SubArray: " + ex.Message);
        }
        return result;
    }

    public static T[] SubArray<T>(T[] arr, int start)
    {
        return SubArray(arr, start, arr.Length - start);
    }
}