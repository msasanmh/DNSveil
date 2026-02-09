using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace MsmhToolsClass;

public class EncodingTool
{
    private const int MaxByteArraySize_SingleDimension = 2147483591;
    private const int MaxByteArraySize_OtherTypes = 2146435071;

    public static bool TryGetSHA1(string text, out string output)
    {
        output = string.Empty;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA1 hash = SHA1.Create();
            
            int bufferSize = 20;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool isSuccess = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (isSuccess)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                output = Convert.ToHexString(hashBuffer);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryGetSHA1: " + ex.Message);
        }

        return false;
    }

    public static bool TryGetSHA256(string text, out string output)
    {
        output = string.Empty;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA256 hash = SHA256.Create();

            int bufferSize = 32;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool isSuccess = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (isSuccess)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                output = Convert.ToHexString(hashBuffer);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryGetSHA256: " + ex.Message);
        }

        return false;
    }

    public static bool TryGetSHA384(string text, out string output)
    {
        output = string.Empty;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA384 hash = SHA384.Create();

            int bufferSize = 48;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool isSuccess = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (isSuccess)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                output = Convert.ToHexString(hashBuffer);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryGetSHA384: " + ex.Message);
        }

        return false;
    }

    public static bool TryGetSHA512(string text, out string output)
    {
        output = string.Empty;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using SHA512 hash = SHA512.Create();
            
            int bufferSize = 64;
            Span<byte> hashBuffer = new(new byte[bufferSize]);
            bool isSuccess = hash.TryComputeHash(buffer, hashBuffer, out int bytesWritten);
            if (isSuccess)
            {
                hashBuffer = hashBuffer[..bytesWritten];
                output = Convert.ToHexString(hashBuffer);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryGetSHA512: " + ex.Message);
        }

        return false;
    }

    private static int GetBufferSize_FromBase64String(string? encodedString)
    {
        if (string.IsNullOrEmpty(encodedString)) return 0;
        // The Formula Ensures The Buffer Is Not Too Large Or Too Small.
        int bufferSize = (encodedString.Length * 3) / 4 - (encodedString.EndsWith("==") ? 2 : encodedString.EndsWith("=") ? 1 : 0);
        if (bufferSize > MaxByteArraySize_SingleDimension) bufferSize = MaxByteArraySize_SingleDimension;
        return bufferSize;
    }

    private static int GetBufferSize_ToBase64String(byte[] buffer)
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

    public static bool TryEncodeBase64(string plainText, out string encodedString)
    {
        encodedString = string.Empty;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(plainText);
            int bufferSize = GetBufferSize_ToBase64String(buffer);
            char[] base64Buffer = new char[bufferSize];
            bool isSuccess = Convert.TryToBase64Chars(buffer, base64Buffer, out int charsWritten);
            if (isSuccess)
            {
                encodedString = new(base64Buffer, 0, charsWritten);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryEncodeBase64: " + ex.Message);
        }

        return false;
    }

    public static bool TryDecodeBase64(string base64, out byte[] output)
    {
        output = Array.Empty<byte>();

        try
        {
            int bufferSize = GetBufferSize_FromBase64String(base64);
            Span<byte> buffer = new(new byte[bufferSize]);
            bool isSuccess = Convert.TryFromBase64String(base64, buffer, out int bytesWritten);
            if (isSuccess)
            {
                output = buffer[..bytesWritten].ToArray();
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryDecodeBase64: " + ex.Message);
            Debug.WriteLine("EncodingTool TryDecodeBase64 Base64: " + base64);
        }

        return false;
    }

    public static bool TryDecodeBase64(string base64, out string output)
    {
        output = string.Empty;

        try
        {
            bool isSuccess = TryDecodeBase64(base64, out byte[] buffer);
            if (isSuccess)
            {
                output = Encoding.UTF8.GetString(buffer);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryDecodeBase64 (out string): " + ex.Message);
        }

        return false;
    }

    public static string Base64ToBase64Url(string base64)
    {
        try
        {
            return base64.Replace("=", "").Replace("/", "_").Replace("+", "-");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool Base64ToBase64Url: " + ex.Message);
            return string.Empty;
        }
    }

    public static string Base64UrlToBase64(string base64Url)
    {
        try
        {
            base64Url = base64Url.ReplaceLineEndings();
            base64Url = base64Url.Replace("\\n", Environment.NewLine);
            base64Url = base64Url.Replace(Environment.NewLine, "");
            base64Url = base64Url.Replace("_", "/").Replace("-", "+").Replace(" ", "");
            base64Url = base64Url.PadRight(base64Url.Length + (4 - base64Url.Length % 4) % 4, '=');
            return base64Url;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool Base64UrlToBase64: " + ex.Message);
            return string.Empty;
        }
    }

    public static bool TryEncodeBase64Url(byte[] buffer, out string output)
    {
        output = string.Empty;

        try
        {
            int bufferSize = GetBufferSize_ToBase64String(buffer);
            char[] base64Buffer = new char[bufferSize];
            bool isSuccess = Convert.TryToBase64Chars(buffer, base64Buffer, out int charsWritten);
            if (isSuccess)
            {
                string base64 = new(base64Buffer, 0, charsWritten);
                output = Base64ToBase64Url(base64);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryEncodeBase64Url: " + ex.Message);
        }

        return false;
    }

    public static bool TryEncodeBase64Url(string base64Url, out string output)
    {
        output = string.Empty;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(base64Url);
            bool isSuccess = TryEncodeBase64Url(buffer, out string encodedString);
            if (isSuccess)
            {
                output = encodedString;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryEncodeBase64Url (in string): " + ex.Message);
        }

        return false;
    }

    public static bool TryDecodeBase64Url(string base64Url, out byte[] output)
    {
        output = Array.Empty<byte>();
        string base64 = Base64UrlToBase64(base64Url);

        try
        {
            int bufferSize = GetBufferSize_FromBase64String(base64);
            Span<byte> buffer = new(new byte[bufferSize]);
            bool isSuccess = Convert.TryFromBase64String(base64, buffer, out int bytesWritten);
            if (isSuccess)
            {
                output = buffer[..bytesWritten].ToArray();
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryDecodeBase64Url: " + ex.Message);
            Debug.WriteLine("EncodingTool TryDecodeBase64Url Base64Url: " + base64Url);
            Debug.WriteLine("EncodingTool TryDecodeBase64Url Base64: " + base64);
        }

        return false;
    }

    public static bool TryDecodeBase64Url(string base64Url, out string output)
    {
        output = string.Empty;

        try
        {
            bool isSuccess = TryDecodeBase64Url(base64Url, out byte[] buffer);
            if (isSuccess)
            {
                output = Encoding.UTF8.GetString(buffer);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EncodingTool TryDecodeBase64Url (out string): " + ex.Message);
        }

        return false;
    }

}