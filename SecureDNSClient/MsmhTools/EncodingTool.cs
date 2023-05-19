using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools
{
    public class EncodingTool
    {
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
