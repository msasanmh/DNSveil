using System;
using System.Runtime.Serialization;

#nullable enable
namespace MsmhToolsClass.HTTPProxyServer
{
    /// <summary>
    /// HTTP methods, i.e. GET, PUT, POST, DELETE, etc.
    /// </summary>
    public enum HttpMethodReq
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
}
