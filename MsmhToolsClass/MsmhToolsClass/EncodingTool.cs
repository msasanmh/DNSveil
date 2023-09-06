using System;
using System.Security.Cryptography;
using System.Text;

namespace MsmhToolsClass
{
    public class EncodingTool
    {
        public static string GetSHA1(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA1 hash = SHA1.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }

        public static string GetSHA256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA256 hash = SHA256.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }

        public static string GetSHA384(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA384 hash = SHA384.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }

        public static string GetSHA512(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using SHA512 hash = SHA512.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            return Convert.ToHexString(hashedInputBytes);
        }

        public static string Base64Encode(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            string encodedString = Convert.ToBase64String(data);
            return encodedString;
        }

        public static string Base64Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static string UrlEncode(byte[] arg)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg));

            var s = Convert.ToBase64String(arg);
            return s.Replace("=", "").Replace("/", "_").Replace("+", "-");
        }

        public static string UrlToBase64(string arg)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg));

            var s = arg.PadRight(arg.Length + (4 - arg.Length % 4) % 4, '=').Replace("_", "/").Replace("-", "+");
            return s;
        }

        public static byte[] UrlDecode(string arg)
        {
            var decrypted = UrlToBase64(arg);
            return Convert.FromBase64String(decrypted);
        }

        public static T[] SubArray<T>(T[] arr, int start, int length)
        {
            var result = new T[length];
            Buffer.BlockCopy(arr, start, result, 0, length);

            return result;
        }

        public static T[] SubArray<T>(T[] arr, int start)
        {
            return SubArray(arr, start, arr.Length - start);
        }
    }

}
