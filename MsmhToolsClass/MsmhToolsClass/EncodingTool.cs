using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace MsmhToolsClass;

public class EncodingTool
{
    private const int MaxByteArraySize_SingleDimension = 2147483591;
    private const int MaxByteArraySize_OtherTypes = 2146435071;

    public static string GetSHA1(string text)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA1 hash = SHA1.Create();
            
            int bufferSize = 20;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool success = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (success)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                return Convert.ToHexString(hashBuffer);
            }
            return string.Empty;
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
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA256 hash = SHA256.Create();

            int bufferSize = 32;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool success = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (success)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                return Convert.ToHexString(hashBuffer);
            }
            return string.Empty;
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
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA384 hash = SHA384.Create();

            int bufferSize = 48;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool success = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (success)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                return Convert.ToHexString(hashBuffer);
            }
            return string.Empty;
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
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA512 hash = SHA512.Create();
            
            int bufferSize = 64;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool success = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (success)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                return Convert.ToHexString(hashBuffer);
            }
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static int GetBufferSize_FromBase64String(string? encodedString)
    {
        if (string.IsNullOrEmpty(encodedString)) return 0;
        // The Formula Ensures The Buffer Is Not Too Large Or Too Small.
        int bufferSize = (encodedString.Length * 3) / 4 - (encodedString.EndsWith("==") ? 2 : encodedString.EndsWith("=") ? 1 : 0);
        if (bufferSize > MaxByteArraySize_SingleDimension) bufferSize = MaxByteArraySize_SingleDimension;
        return bufferSize;
    }

    public static int GetBufferSize_ToBase64String(byte[] buffer)
    {
        // The Formula Ensures The Buffer Is Not Too Large Or Too Small.
        int bufferSize = ((buffer.Length * 4) / 3) + 4; // +4 To Ensure Space For Padding.
        if (bufferSize > MaxByteArraySize_SingleDimension) bufferSize = MaxByteArraySize_SingleDimension;
        return bufferSize;
    }

    public static bool IsBase64String(string? encodedString)
    {
        try
        {
            if (string.IsNullOrEmpty(encodedString)) return false;
            int bufferSize = GetBufferSize_FromBase64String(encodedString);
            Span<byte> buffer = new(new byte[bufferSize]);
            return Convert.TryFromBase64String(encodedString, buffer, out int _);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string Base64Encode(string plainText)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(plainText);
            int bufferSize = GetBufferSize_ToBase64String(buffer);
            char[] base64Buffer = new char[bufferSize];
            bool success = Convert.TryToBase64Chars(buffer, base64Buffer, out int charsWritten);
            if (success) return new(base64Buffer, 0, charsWritten);
            return string.Empty;
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
            int bufferSize = GetBufferSize_FromBase64String(encodedString);
            Span<byte> buffer = new(new byte[bufferSize]);
            bool success = Convert.TryFromBase64String(encodedString, buffer, out int bytesWritten);
            if (success)
            {
                buffer = buffer[..bytesWritten];
                return Encoding.UTF8.GetString(buffer);
            }
            return string.Empty;
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
            base64Url = base64Url.ReplaceLineEndings();
            base64Url = base64Url.Replace(Environment.NewLine, "");
            base64Url = base64Url.Replace("_", "/").Replace("-", "+").Replace(" ", "");
            base64Url = base64Url.PadRight(base64Url.Length + (4 - base64Url.Length % 4) % 4, '=');
            return base64Url;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string Base64UrlEncode(byte[] buffer)
    {
        try
        {
            int bufferSize = GetBufferSize_ToBase64String(buffer);
            char[] base64Buffer = new char[bufferSize];
            bool success = Convert.TryToBase64Chars(buffer, base64Buffer, out int charsWritten);
            if (success)
            {
                string base64 = new(base64Buffer, 0, charsWritten);
                return Base64ToBase64Url(base64);
            }
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static byte[] Base64UrlDecode(string base64Url)
    {
        string base64 = Base64UrlToBase64(base64Url);

        try
        {
            int bufferSize = GetBufferSize_FromBase64String(base64);
            Span<byte> buffer = new(new byte[bufferSize]);
            bool success = Convert.TryFromBase64String(base64, buffer, out int bytesWritten);
            if (success) return buffer[..bytesWritten].ToArray();
            return Array.Empty<byte>();
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