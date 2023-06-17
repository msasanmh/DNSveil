using System;
using System.Runtime.Serialization;

namespace MsmhTools.HTTPProxyServer
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
        public static HttpMethodReq Parse(string method)
        {
            method = method.Trim().ToLower();
            var httpMethod = method switch
            {
                "get" => HttpMethodReq.GET,
                "head" => HttpMethodReq.HEAD,
                "put" => HttpMethodReq.PUT,
                "post" => HttpMethodReq.POST,
                "delete" => HttpMethodReq.DELETE,
                "patch" => HttpMethodReq.PATCH,
                "connect" => HttpMethodReq.CONNECT,
                "options" => HttpMethodReq.OPTIONS,
                "trace" => HttpMethodReq.TRACE,
                _ => HttpMethodReq.UNKNOWN,
            };
            return httpMethod;
        }
    }
}
