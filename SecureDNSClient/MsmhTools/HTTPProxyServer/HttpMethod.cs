using System;
using System.Runtime.Serialization;

namespace MsmhTools.HTTPProxyServer
{
    /// <summary>
    /// HTTP methods, i.e. GET, PUT, POST, DELETE, etc.
    /// </summary>
    public enum HttpMethod
    {
        [EnumMember(Value = "GET")]
        GET,
        [EnumMember(Value = "HEAD")]
        HEAD,
        [EnumMember(Value = "PUT")]
        PUT,
        [EnumMember(Value = "POST")]
        POST,
        [EnumMember(Value = "DELETE")]
        DELETE,
        [EnumMember(Value = "PATCH")]
        PATCH,
        [EnumMember(Value = "CONNECT")]
        CONNECT,
        [EnumMember(Value = "OPTIONS")]
        OPTIONS,
        [EnumMember(Value = "TRACE")]
        TRACE,
        [EnumMember(Value = "UNKNOWN")]
        UNKNOWN
    }

    public static class GetHttpMethod
    {
        public static HttpMethod Parse(string method)
        {
            method = method.Trim().ToLower();
            var httpMethod = method switch
            {
                "get" => HttpMethod.GET,
                "head" => HttpMethod.HEAD,
                "put" => HttpMethod.PUT,
                "post" => HttpMethod.POST,
                "delete" => HttpMethod.DELETE,
                "patch" => HttpMethod.PATCH,
                "connect" => HttpMethod.CONNECT,
                "options" => HttpMethod.OPTIONS,
                "trace" => HttpMethod.TRACE,
                _ => HttpMethod.UNKNOWN,
            };
            return httpMethod;
        }
    }
}
