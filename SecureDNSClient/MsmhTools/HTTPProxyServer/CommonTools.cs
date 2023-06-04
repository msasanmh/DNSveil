using System;

namespace MsmhTools.HTTPProxyServer
{
    internal static class CommonTools
    {
        internal static byte[] AppendBytes(byte[] orig, byte[] append)
        {
            if (append == null) return orig;
            if (orig == null) return append;

            byte[] ret = new byte[orig.Length + append.Length];
            Buffer.BlockCopy(orig, 0, ret, 0, orig.Length);
            Buffer.BlockCopy(append, 0, ret, orig.Length, append.Length);
            return ret;
        }

        /// <summary>
        /// Fully read a stream into a byte array.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>A byte array containing the data read from the stream.</returns>
        public static byte[] StreamToBytes(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

            byte[] buffer = new byte[16 * 1024];
            using MemoryStream ms = new();
            int read;

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Add a key-value pair to a supplied Dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        /// <param name="existing">An existing dictionary.</param>
        /// <returns>The existing dictionary with a new key and value, or, a new dictionary with the new key value pair.</returns>
        public static Dictionary<string, string> AddToDict(string key, string? val, Dictionary<string, string> existing)
        {
            if (string.IsNullOrEmpty(key)) return existing;

            Dictionary<string, string> ret = new();

            if (existing == null)
            {
                ret.Add(key, val);
                return ret;
            }
            else
            {
                if (existing.ContainsKey(key))
                {
                    if (string.IsNullOrEmpty(val)) return existing;
                    string tempVal = existing[key];
                    tempVal += "," + val;
                    existing.Remove(key);
                    existing.Add(key, tempVal);
                    return existing;
                }
                else
                {
                    existing.Add(key, val);
                    return existing;
                }
            }
        }

        public static bool IsWin7()
        {
            bool result = false;
            OperatingSystem os = Environment.OSVersion;
            Version vs = os.Version;

            if (os.Platform == PlatformID.Win32NT)
            {
                if (vs.Minor == 1 && vs.Major == 6)
                    result = true;
            }

            return result;
        }

    }
}
