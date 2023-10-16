using System;
using System.Net;
using System.Net.Sockets;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools;

/// <summary>
/// Provides extension methods around <see cref="DnsMessage"/>.
/// </summary>
public static class DnsQueryFactory
{
    /// <summary>
    /// Generate a unique ID to identify this DNS message.
    /// </summary>
    /// <returns>A random <see cref="ushort"/> value.</returns>
    public static ushort GenerateId()
    {
        var offset = 0;
        return DnsByteExtensions.ReadUInt16(Guid.NewGuid().ToByteArray(), ref offset);
    }

    /// <summary>
    /// Create a DNS query using the specified host name and DNS query type.
    /// </summary>
    /// <param name="host">The DNS host to request in the query.</param>
    /// <param name="type">The type of DNS query to request.</param>
    /// <returns>The complete DNS query.</returns>
    public static DnsMessage CreateQuery(string host, DnsQueryType type = DnsQueryType.A)
    {
        return new DnsMessage
        {
            Header = new DnsHeader
            {
                Id = GenerateId(),
                Host = host,
                QueryType = type,
                QueryClass = DnsQueryClass.IN,
                OperationCode = DnsOperationCode.QUERY,
                QuestionCount = 1,
                RecursionDesired = true
            }
        };
    }

    public static DnsHeader Clone(DnsHeader header)
    {
        return new DnsHeader
        {
            Id = header.Id,
            Host = header.Host,
            QueryType = header.QueryType,
            QueryClass = header.QueryClass,
            OperationCode = header.OperationCode,
            QuestionCount = header.QuestionCount,
            RecursionDesired = header.RecursionDesired,
            AdditionalRecordCount = header.AdditionalRecordCount,
            AnswerRecordCount = header.AnswerRecordCount,
            AuthoritativeAnswer = header.AuthoritativeAnswer,
            IsQueryResponse = header.IsQueryResponse,
            NameServerRecordCount = header.NameServerRecordCount,
            RecursionAvailable = header.RecursionAvailable,
            ResponseCode = header.ResponseCode,
            Truncation = header.Truncation
        };
    }

    public static DnsMessage TruncateAnswer(DnsMessage query)
    {
        var header = Clone(query.Header);

        header.AnswerRecordCount = 0;
        header.NameServerRecordCount = 0;
        header.AdditionalRecordCount = 0;
        header.IsQueryResponse = true;
        header.OperationCode = DnsOperationCode.QUERY;
        header.Truncation = true;

        return new DnsMessage { Header = header };
    }

    /// <summary>
    /// Create a reverse DNS query which resolves an IP address to a host name.
    /// </summary>
    /// <param name="ipAddress">The IPv4 or IPv6 address to resolve.</param>
    /// <returns>The correctly formatted <see cref="DnsQueryType.PTR"/> DNS query.</returns>
    public static DnsMessage CreateReverseQuery(IPAddress ipAddress)
    {
        return CreateQuery(GetReverseLookupHostForIpAddress(ipAddress), DnsQueryType.PTR);
    }

    private static string GetReverseLookupHostForIpAddress(IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return string.Join(".", string.Concat(ipAddress.GetAddressBytes().Select(x => x.ToString("x2"))).Reverse()) + ".ip6.arpa";
        }
        else if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            return string.Join(".", ipAddress.ToString().Split('.').Reverse()) + ".in-addr.arpa";
        }
        else
        {
            throw new InvalidOperationException($"Invalid address type: {ipAddress.AddressFamily}");
        }
    }
}