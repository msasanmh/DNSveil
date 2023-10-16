using System;
using System.Collections.Specialized;

namespace MsmhToolsClass.MsmhProxyServer;

public static class CommonTools
{
    public static class GetHttpMethod
    {
        public static HttpMethod Parse(string method)
        {
            method = method.Trim().ToLower();
            var httpMethod = method switch
            {
                "get" => HttpMethod.Get,
                "head" => HttpMethod.Head,
                "put" => HttpMethod.Put,
                "post" => HttpMethod.Post,
                "connect" => HttpMethod.Post,
                "delete" => HttpMethod.Delete,
                "patch" => HttpMethod.Patch,
                "options" => HttpMethod.Options,
                "trace" => HttpMethod.Trace,
                _ => HttpMethod.Get,
            };
            return httpMethod;
        }
    }

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
    /// Add a key-value pair to a supplied Dictionary.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="existing">An existing dictionary.</param>
    /// <returns>The existing dictionary with a new key and value, or, a new dictionary with the new key value pair.</returns>
    internal static NameValueCollection AddToDict(string key, string? value, NameValueCollection existing)
    {
        if (string.IsNullOrEmpty(key)) return existing;
        if (string.IsNullOrEmpty(value)) value = string.Empty;

        NameValueCollection ret = new();

        if (existing == null)
        {
            ret.Add(key, value);
            return ret;
        }
        else
        {
            string? theKey = existing[key];
            if (!string.IsNullOrEmpty(theKey)) // Key Exist
            {
                if (string.IsNullOrEmpty(value)) return existing;
                string tempVal = theKey;
                tempVal += "," + value;
                existing.Remove(key);
                existing.Add(key, tempVal);
                return existing;
            }
            else
            {
                existing.Add(key, value);
                return existing;
            }
        }
    }

}
