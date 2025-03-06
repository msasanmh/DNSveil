using System.Diagnostics;
using System.Net;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyRequest
{
    // More than this means there is no internet or destination is blocked so it's better to cancel the request
    private static readonly int TimeoutRequestCreationMS = 500;

    public static async Task<ProxyRequest?> RequestHTTP_S(byte[] firstBuffer, CancellationToken ct)
    {
        Task<ProxyRequest?> task = Task.Run(() =>
        {
            try
            {
                // Set Security Protocols
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                HttpRequestResult hrResult = HttpRequest.Read(firstBuffer);
                if (!hrResult.IsSuccess) return null;
                if (hrResult.URI == null) return null;

                HttpRequest httpRequest = new()
                {
                    IsForHttpProxy = true,
                    URI = hrResult.URI,
                    Method = hrResult.Method,
                    Headers = hrResult.Headers,
                    UserAgent = hrResult.UserAgent,
                    TimeoutMS = 30000
                };
                
                Proxy.Name proxyName = httpRequest.URI.Scheme.Equals("http") ? Proxy.Name.HTTP : Proxy.Name.HTTPS;
                if (httpRequest.UserAgent.Equals("DNSveil - A Secure DNS Client", StringComparison.OrdinalIgnoreCase)) proxyName = Proxy.Name.Test;
                
                // I Set User and Pass to none (I don't support Auth)
                string user = string.Empty, pass = string.Empty;

                // Create Request
                ProxyRequest proxyRequest = new(proxyName, Socks.Version.Zero, Socks.Commands.Unknown, Socks.AddressType.Domain, httpRequest.URI.Host, httpRequest.URI.Port, user, pass);
                proxyRequest.HttpMethod = httpRequest.Method;
                proxyRequest.HttpRequest = httpRequest;
                return proxyRequest;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RequestHTTP_S: " + ex.Message);
                return null;
            }
        });

        try
        {
            return await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutRequestCreationMS), ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<ProxyRequest?> RequestSocks4_4A(ProxyClient client, byte[] firstBuffer, CancellationToken ct)
    {
        Task<ProxyRequest?> task = Task.Run(async () =>
        {
            try
            {
                async Task SendSocks4Reply(ProxyClient socksClient, Socks.Version version, Socks.Status status, IReadOnlyList<byte> portBuffer, IReadOnlyList<byte> addressBuffer)
                {
                    byte[] response = new byte[]
                    {
                        (byte)version,
                        (byte)status,
                        portBuffer[0], portBuffer[1],
                        addressBuffer[0], addressBuffer[1], addressBuffer[2], addressBuffer[3],
                    };

                    await socksClient.SendAsync(response).ConfigureAwait(false);
                }

                bool IsSocks4aProtocol(IReadOnlyList<byte> ip)
                {
                    return ip[0] == 0 && ip[1] == 0 && ip[2] == 0 && ip[3] != 0;
                }

                // Set Proxy Name
                Proxy.Name proxyName = Proxy.Name.Socks4;

                // Get Version
                if (firstBuffer[0] != (byte)Socks.Version.Socks4) return null;
                Socks.Version version = Socks.Version.Socks4;

                // Get Command
                Socks.Commands command = firstBuffer[1] switch
                {
                    (byte)Socks.Commands.Connect => Socks.Commands.Connect,
                    (byte)Socks.Commands.Bind => Socks.Commands.Bind,
                    _ => Socks.Commands.Unknown
                };

                if (command == Socks.Commands.Unknown) return null;

                // The AddressType of Socks4 is always Ipv4

                // Get Port
                byte[] portBuffer = new[] { firstBuffer[2], firstBuffer[3] };
                ushort port = (ushort)(portBuffer[0] << 8 | portBuffer[1]);

                // Get Address
                byte[] addressBuffer = new[] { firstBuffer[4], firstBuffer[5], firstBuffer[6], firstBuffer[7] };
                string address = new IPAddress(addressBuffer).ToString();

                // Get User ID
                byte[] userIdBuffer = Array.Empty<byte>();
                string userId = string.Empty;
                int skipToUserId = 8;
                if (firstBuffer.Length > skipToUserId)
                {
                    byte[] beginningOfUserId = firstBuffer.Skip(skipToUserId).ToArray();

                    for (int n = 0; n < beginningOfUserId.Length; n++)
                    {
                        byte b = beginningOfUserId[n];
                        if (b == 0x00) break;
                        userIdBuffer = userIdBuffer.Concat(new byte[] { b }).ToArray();
                    }

                    userId = Encoding.UTF8.GetString(userIdBuffer);
                }

                // Get Password
                // Oops Sock4 doesn't support password

                // Socks4A
                if (IsSocks4aProtocol(addressBuffer))
                {
                    byte[] hostnameBuffer = Array.Empty<byte>();
                    string hostname = string.Empty;
                    int skipToHostname = skipToUserId + userIdBuffer.Length + 1; // 1 is Null Byte Terminator
                    if (firstBuffer.Length > skipToHostname)
                    {
                        byte[] beginningOfHostname = firstBuffer.Skip(skipToHostname).ToArray();

                        for (int n = 0; n < beginningOfHostname.Length; n++)
                        {
                            byte b = beginningOfHostname[n];
                            if (b == 0x00) break;
                            hostnameBuffer = hostnameBuffer.Concat(new byte[] { b }).ToArray();
                        }

                        hostname = Encoding.UTF8.GetString(hostnameBuffer);
                        address = hostname;
                        proxyName = Proxy.Name.Socks4A; // It's Socks 4A
                    }
                }

                // Send Response
                await SendSocks4Reply(client, Socks.Version.Zero, Socks.Status.GrantedSocks4, portBuffer, addressBuffer).ConfigureAwait(false);

                // Create Socks Request
                return new ProxyRequest(proxyName, version, command, Socks.AddressType.Ipv4, address, port, userId, string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RequestSocks4_4A: " + ex.Message);
                return null;
            }
        });

        try
        {
            return await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutRequestCreationMS), ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<ProxyRequest?> RequestSocks5(ProxyClient client, byte[] firstBuffer, CancellationToken ct)
    {
        Task<ProxyRequest?> task = Task.Run(async () =>
        {
            try
            {
                // Get Version
                if (firstBuffer[0] != (byte)Socks.Version.Socks5) return null;

                // Get Methods
                int lenOfMethods = firstBuffer[1];
                byte[] methodsBuffer = new byte[lenOfMethods];
                Buffer.BlockCopy(firstBuffer, 1, methodsBuffer, 0, lenOfMethods);

                List<byte> methodsList = new();
                for (int n = 0; n < methodsBuffer.Length; n++)
                {
                    byte method = methodsBuffer[n];
                    methodsList.Add(method);
                }

                // Auth Not Supported
                //if (!methodsList.Contains((byte)Socks5.HandshakeMethods.NoAuth)) return null;

                // Send Response
                byte[] response = new[] { (byte)Socks.Version.Socks5, (byte)Socks.HandshakeMethods.NoAuth };
                await client.SendAsync(response).ConfigureAwait(false);

                // Connection Request
                byte[] buffer = new byte[MsmhAgnosticServer.MaxDataSize];
                int recv = await client.ReceiveAsync(buffer).ConfigureAwait(false);
                if (recv == -1) return null; // recv = -1 Will Result in Overflow

                byte[] buff = new byte[recv];
                Buffer.BlockCopy(buffer, 0, buff, 0, recv);

                // Read Connection Request to Create SocksRequest
                if ((Socks.Version)buff[0] != Socks.Version.Socks5) return null;

                // Get Command
                Socks.Commands command = buff[1] switch
                {
                    (byte)Socks.Commands.Connect => Socks.Commands.Connect,
                    (byte)Socks.Commands.Bind => Socks.Commands.Bind,
                    (byte)Socks.Commands.UDP => Socks.Commands.UDP,
                    _ => Socks.Commands.Unknown
                };

                if (command == Socks.Commands.Unknown) return null;

                // buff[2] is RSV and it's always 0x00

                // Get AddressType (ATYP)
                Socks.AddressType addressType = buff[3] switch
                {
                    (byte)Socks.AddressType.Domain => Socks.AddressType.Domain,
                    (byte)Socks.AddressType.Ipv4 => Socks.AddressType.Ipv4,
                    (byte)Socks.AddressType.Ipv6 => Socks.AddressType.Ipv6,
                    _ => Socks.AddressType.Unknown
                };

                if (addressType == Socks.AddressType.Unknown) return null;

                // Get Address
                string address = string.Empty;
                byte[]? addressBuffer = null;
                if (addressType == Socks.AddressType.Domain)
                {
                    int lenOfDomain = buff[4]; // Convert.ToInt32(buff[4]);
                    addressBuffer = new byte[lenOfDomain];
                    Buffer.BlockCopy(buff, 5, addressBuffer, 0, lenOfDomain);
                    address = Encoding.UTF8.GetString(addressBuffer);
                }
                else if (addressType == Socks.AddressType.Ipv4)
                {
                    addressBuffer = new[] { buff[4], buff[5], buff[6], buff[7] };
                    address = new IPAddress(addressBuffer).ToString();
                }
                else if (addressType == Socks.AddressType.Ipv6)
                {
                    addressBuffer = new[] { buff[4], buff[5], buff[6], buff[7],
                                        buff[8], buff[9], buff[10], buff[11],
                                        buff[12], buff[13], buff[14], buff[15],
                                        buff[16], buff[17], buff[18], buff[19]};
                    string ipv6 = new IPAddress(addressBuffer).ToString();
                    address = $"{ipv6}";
                }

                if (string.IsNullOrEmpty(address) || addressBuffer == null) return null;

                // Get Port
                byte[] portBuffer = new byte[2];
                Buffer.BlockCopy(buff, buff.Length - 2, portBuffer, 0, 2);
                int port = (portBuffer[0] << 8) + portBuffer[1];

                // I Set User and Pass to none (I don't support Auth)
                string user = string.Empty, pass = string.Empty;

                // Create Socks Request
                return new ProxyRequest(Proxy.Name.Socks5, Socks.Version.Socks5, command, addressType, address, port, user, pass);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RequestSocks5: " + ex.Message);
                return null;
            }
        });

        try
        {
            return await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutRequestCreationMS), ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<ProxyRequest?> RequestSniProxy(string sni, int port, CancellationToken ct)
    {
        Task<ProxyRequest?> task = Task.Run(() =>
        {
            try
            {
                // Sni Proxy Doesn't Have A User And Pass
                string user = string.Empty, pass = string.Empty;

                // Create Request
                ProxyRequest proxyRequest = new(Proxy.Name.SniProxy, Socks.Version.Zero, Socks.Commands.Unknown, Socks.AddressType.Domain, sni, port, user, pass);
                return proxyRequest;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RequestSniProxy: " + ex.Message);
                return null;
            }
        });

        try
        {
            return await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutRequestCreationMS), ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public ProxyRequest(Proxy.Name proxyName, Socks.Version version, Socks.Commands command, Socks.AddressType addressType, string address, int port, string user, string pass)
    {
        ProxyName = proxyName;
        Version = version;
        Command = command;
        AddressType = addressType;
        Address = address;
        AddressOrig = address;
        AddressSNI = address;
        Port = port;
        UserId = user;
        Password = pass;
        Status = Version == Socks.Version.Socks5 ? Socks.Status.Granted : Socks.Status.GrantedSocks4;
    }

    public Proxy.Name ProxyName { get; set; }
    public Socks.Version Version { get; set; }
    public Socks.AddressType AddressType { get; set; }
    public Socks.Commands Command { get; private set; }
    public IPAddress ClientIP { get; set; } = IPAddress.None;
    public string Address { get; set; }
    public string AddressOrig { get; set; }
    public string AddressSNI { get; set; }
    public bool AddressIsIp => NetworkTool.IsIP(AddressOrig, out _);
    public int Port { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }
    public Socks.Status Status { get; set; }

    /// <summary>
    /// Only For HTTP and HTTPS
    /// </summary>
    public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

    /// <summary>
    /// For HTTP Only
    /// </summary>
    public HttpRequest HttpRequest { get; set; } = new();

    /// <summary>
    /// Close request if didn't receive data for n seconds. Default: 0 Sec (Disabled)
    /// </summary>
    public int TimeoutSec { get; set; } = 0;

    /// <summary>
    /// Apply Fragment to this Request if Fragment Program is available.
    /// </summary>
    public bool ApplyFragment { get; set; } = false;

    /// <summary>
    /// Apply ChangeSNI to this Request if ChangeSNI is available.
    /// </summary>
    public bool ApplyChangeSNI { get; set; } = false;

    public bool IsDestBlocked { get; set; } = false;

    public bool ApplyUpstreamProxy { get; set; } = false;
    public bool ApplyUpstreamProxyToBlockedIPs { get; set; } = false;
    public string? UpstreamProxyScheme { get; set; }
    public string? UpstreamProxyUser { get; set; }
    public string? UpstreamProxyPass { get; set; }
    //public AgnosticProgram.ProxyRules.ProxyRulesResult RulesResult { get; set; } = new();

    /// <summary>
    /// Only for Socks5
    /// </summary>
    public byte[] GetConnectionRequestFrameData()
    {
        try
        {
            // Read SocksRequest to Create Connection Request Frame Data
            byte[] response = new byte[]
            {
                (byte)Version,
                (byte)Status,
                (byte)Socks.Version.Zero, // RSV it's always 0x00
                (byte)AddressType
            };

            // Get AddressBuffer
            byte[] addressBuffer;
            if (AddressType == Socks.AddressType.Domain)
            {
                // Get AddressOnlyBuffer
                byte[] addressOnlyBuffer = Encoding.UTF8.GetBytes(Address);

                // Get Length of AddressBuffer
                byte[] lenOfAddressBuffer = new byte[] { (byte)addressOnlyBuffer.Length };

                addressBuffer = lenOfAddressBuffer.Concat(addressOnlyBuffer).ToArray();
            }
            else if (AddressType == Socks.AddressType.Ipv4 || AddressType == Socks.AddressType.Ipv6)
            {
                addressBuffer = IPAddress.Parse(Address).GetAddressBytes();
            }
            else
            {
                addressBuffer = Encoding.UTF8.GetBytes(Address);
            }

            // Get Port Buffer
            int port = IPAddress.NetworkToHostOrder(Port);
            byte[] portbuffer = new byte[] { (byte)port, (byte)(port >> 8) };
            
            return response.Concat(addressBuffer).Concat(portbuffer).ToArray();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetConnectionRequestFrameData: " + ex.Message);
            return Array.Empty<byte>();
        }
    }

}