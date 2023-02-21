using System;
using System.Text;
using System.Security.Cryptography;
using Force.Crc32;
using System.Text.RegularExpressions;

namespace MsmhTools
{
    public class Texts
    {
        //-----------------------------------------------------------------------------------
        public static string GetCRC32(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            uint crc32 = Crc32Algorithm.Compute(bytes);
            return crc32.ToString();
        }
        //-----------------------------------------------------------------------------------
        public static string GetSHA512(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            using var hash = SHA512.Create();
            var hashedInputBytes = hash.ComputeHash(bytes);
            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (var b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("X2"));
            return hashedInputStringBuilder.ToString();
        }
        //-----------------------------------------------------------------------------------
        public static string? GetTextByLineNumber(string text, int lineNo)
        {
            string[] lines = text.Replace("\r", "").Split('\n');
            return lines.Length >= lineNo ? lines[lineNo - 1] : null;
        }
        //-----------------------------------------------------------------------------------
        public static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}
