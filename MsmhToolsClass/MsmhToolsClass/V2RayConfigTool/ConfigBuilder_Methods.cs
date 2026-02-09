using System.Diagnostics;
using System.Net;
using System.Text.Json;
using MsmhToolsClass.V2RayConfigTool.Inbounds;
using MsmhToolsClass.V2RayConfigTool.Outbounds;
using static MsmhToolsClass.V2RayConfigTool.XrayConfig;

namespace MsmhToolsClass.V2RayConfigTool;

public partial class ConfigBuilder
{
    public enum Protocol
    {
        Unknown = 0,
        Vless = 1,
        Vmess = 2,
        ShadowSocks = 3,
        Trojan = 4,
        WireGuard = 5,
        HTTP = 6,
        SOCKS = 7,
        Hysteria = 8, // Not Supported By Xray
        Hysteria2 = 9 // Not Supported By Xray
    }

    public enum Security
    {
        None = 0,
        TLS = 1,
        Reality = 2
    }

    public readonly struct SecurityStruct
    {
        public static readonly string TLS = "tls";
        public static readonly string Reality = "reality";
        public static readonly string None = "none";
    }

    public enum Transport
    {
        TCP = 1, // Default
        H2 = 2,
        XHTTP = 3,
        Quic = 4,
        KCP = 5,
        GRPC = 6,
        WS = 7,
        HttpUpgrade = 8
    }

    public readonly struct TransportStruct
    {
        public static readonly string TCP = "tcp"; // Default
        public static readonly string H2 = "h2"; // = HTTP
        public static readonly string HTTP = "http"; // = H2
        public static readonly string XHTTP = "xhttp";
        public static readonly string Quic = "quic";
        public static readonly string KCP = "kcp";
        public static readonly string GRPC = "grpc";
        public static readonly string WS = "ws";
        public static readonly string HttpUpgrade = "httpupgrade";
    }

    public static Protocol GetProtocolByUrl(string url)
    {
        Protocol protocol = Protocol.Unknown;

        try
        {
            url = url.TrimStart().ToLower();
            if (url.StartsWith("vless://")) protocol = Protocol.Vless;
            else if (url.StartsWith("vmess://")) protocol = Protocol.Vmess;
            else if (url.StartsWith("ss://")) protocol = Protocol.ShadowSocks;
            else if (url.StartsWith("trojan://")) protocol = Protocol.Trojan;
            else if (url.StartsWith("wireguard://") || url.StartsWith("wg://")) protocol = Protocol.WireGuard;
            else if (url.StartsWith("http://")) protocol = Protocol.HTTP;
            else if (url.StartsWith("socks://")) protocol = Protocol.SOCKS;
            else if (url.StartsWith("hysteria://")) protocol = Protocol.Hysteria;
            else if (url.StartsWith("hysteria2://") || url.StartsWith("hy2://")) protocol = Protocol.Hysteria2;
        }
        catch (Exception) { }

        return protocol;
    }

    private static Protocol GetProtocolByConfigOutbound(string outboundProtocol)
    {
        Protocol protocol = Protocol.Unknown;

        try
        {
            if (outboundProtocol.Equals(ConfigOutbound.Get.Protocol.Vless, StringComparison.OrdinalIgnoreCase)) protocol = Protocol.Vless;
            else if (outboundProtocol.Equals(ConfigOutbound.Get.Protocol.Vmess, StringComparison.OrdinalIgnoreCase)) protocol = Protocol.Vmess;
            else if (outboundProtocol.Equals(ConfigOutbound.Get.Protocol.Shadowsocks, StringComparison.OrdinalIgnoreCase)) protocol = Protocol.ShadowSocks;
            else if (outboundProtocol.Equals(ConfigOutbound.Get.Protocol.Trojan, StringComparison.OrdinalIgnoreCase)) protocol = Protocol.Trojan;
            else if (outboundProtocol.Equals(ConfigOutbound.Get.Protocol.Wireguard, StringComparison.OrdinalIgnoreCase)) protocol = Protocol.WireGuard;
            else if (outboundProtocol.Equals(ConfigOutbound.Get.Protocol.Http, StringComparison.OrdinalIgnoreCase)) protocol = Protocol.HTTP;
            else if (outboundProtocol.Equals(ConfigOutbound.Get.Protocol.Socks, StringComparison.OrdinalIgnoreCase)) protocol = Protocol.SOCKS;
        }
        catch (Exception) { }

        return protocol;
    }

    private static Security GetSecurity(string security)
    {
        try
        {
            if (security.Equals(SecurityStruct.TLS, StringComparison.OrdinalIgnoreCase)) return Security.TLS;
            else if (security.Equals(SecurityStruct.Reality, StringComparison.OrdinalIgnoreCase)) return Security.Reality;
            else return Security.None;
        }
        catch (Exception)
        {
            return Security.None;
        }
    }

    private static Transport GetTransport(string transport)
    {
        try
        {
            if (transport.Equals(TransportStruct.TCP, StringComparison.OrdinalIgnoreCase)) return Transport.TCP;
            else if (transport.Equals(TransportStruct.H2, StringComparison.OrdinalIgnoreCase) || transport.Equals(TransportStruct.HTTP, StringComparison.OrdinalIgnoreCase)) return Transport.H2;
            else if (transport.Equals(TransportStruct.XHTTP, StringComparison.OrdinalIgnoreCase)) return Transport.XHTTP;
            else if (transport.Equals(TransportStruct.Quic, StringComparison.OrdinalIgnoreCase)) return Transport.Quic;
            else if (transport.Equals(TransportStruct.KCP, StringComparison.OrdinalIgnoreCase)) return Transport.KCP;
            else if (transport.Equals(TransportStruct.GRPC, StringComparison.OrdinalIgnoreCase)) return Transport.GRPC;
            else if (transport.Equals(TransportStruct.WS, StringComparison.OrdinalIgnoreCase)) return Transport.WS;
            else if (transport.Equals(TransportStruct.HttpUpgrade, StringComparison.OrdinalIgnoreCase)) return Transport.HttpUpgrade;
            else return Transport.TCP;
        }
        catch (Exception)
        {
            return Transport.TCP;
        }
    }

    private static ConfigOutbound.OutboundStreamSettings SetStreamSettings(ConfigOutbound.OutboundStreamSettings streamSettings, Dictionary<string, string> kv, NetworkTool.URL urid)
    {
        // Modify Based On Security
        streamSettings = setStreamSettingsInternal(streamSettings.Security, kv, urid, streamSettings);
        // Modify Based On Transport Layer
        streamSettings = setStreamSettingsInternal(streamSettings.Network, kv, urid, streamSettings);
        return streamSettings;

        static ConfigOutbound.OutboundStreamSettings setStreamSettingsInternal(string netwotkOrSecurity, Dictionary<string, string> kv, NetworkTool.URL urid, ConfigOutbound.OutboundStreamSettings streamSettings)
        {
            try
            {
                Security security = GetSecurity(netwotkOrSecurity);
                Transport transport = GetTransport(netwotkOrSecurity);

                if (security == Security.TLS) // Security
                {
                    streamSettings.TlsSettings ??= new();

                    // ServerName
                    string serverName = kv.GetValueOrDefault("sni", kv.GetValueOrDefault("host", urid.Host));
                    if (!string.IsNullOrEmpty(serverName)) streamSettings.TlsSettings.ServerName = serverName;

                    // AllowInsecure
                    string allowInsecure = kv.GetValueOrDefault("insecure", kv.GetValueOrDefault("allowinsecure", string.Empty));
                    streamSettings.TlsSettings.AllowInsecure = allowInsecure.Equals("1") || allowInsecure.Equals("true", StringComparison.OrdinalIgnoreCase);

                    // Alpn
                    string? alpns = kv.GetValueOrDefault("alpn");
                    if (!string.IsNullOrEmpty(alpns))
                    {
                        List<string> alpn = new();
                        if (alpns.Contains(','))
                        {
                            alpn = alpns.Split(',').ToList();
                        }
                        else alpn.Add(alpns);
                        streamSettings.TlsSettings.Alpn = alpn;
                    }

                    // Fingerprint
                    string fingerprint = kv.GetValueOrDefault("fp", kv.GetValueOrDefault("fingerprint", ConfigOutbound.OutboundStreamSettings.StreamTlsSettings.Get.Fingerprint.Chrome));
                    streamSettings.TlsSettings.Fingerprint = fingerprint;

                }
                else if (security == Security.Reality) // Security
                {
                    streamSettings.RealitySettings ??= new();

                    // Fingerprint
                    string fingerprint = kv.GetValueOrDefault("fp", kv.GetValueOrDefault("fingerprint", ConfigOutbound.OutboundStreamSettings.StreamTlsSettings.Get.Fingerprint.Chrome));
                    streamSettings.RealitySettings.Fingerprint = fingerprint;

                    // ServerName
                    string serverName = kv.GetValueOrDefault("sni", kv.GetValueOrDefault("host", urid.Host));
                    if (!string.IsNullOrEmpty(serverName)) streamSettings.RealitySettings.ServerName = serverName;

                    // AllowInsecure
                    string allowInsecure = kv.GetValueOrDefault("insecure", kv.GetValueOrDefault("allowinsecure", string.Empty));
                    streamSettings.RealitySettings.AllowInsecure = allowInsecure.Equals("1") || allowInsecure.Equals("true", StringComparison.OrdinalIgnoreCase);

                    // Alpn
                    string? alpns = kv.GetValueOrDefault("alpn");
                    if (!string.IsNullOrEmpty(alpns))
                    {
                        List<string> alpn = new();
                        if (alpns.Contains(','))
                        {
                            alpn = alpns.Split(',').ToList();
                        }
                        else alpn.Add(alpns);
                        streamSettings.RealitySettings.Alpn = alpn;
                    }

                    // PublicKey
                    string publicKey = kv.GetValueOrDefault("pbk", string.Empty);
                    if (!string.IsNullOrEmpty(publicKey)) streamSettings.RealitySettings.PublicKey = publicKey;

                    // ShortId
                    string shortID = kv.GetValueOrDefault("sid", string.Empty);
                    if (!string.IsNullOrEmpty(shortID)) streamSettings.RealitySettings.ShortId = shortID;

                    // SpiderX
                    string spiderX = kv.GetValueOrDefault("spx", string.Empty);
                    if (!string.IsNullOrEmpty(spiderX)) streamSettings.RealitySettings.SpiderX = spiderX;

                    // Mldsa65Verify
                    string mldsa65Verify = kv.GetValueOrDefault("pqv", string.Empty);
                    if (!string.IsNullOrEmpty(mldsa65Verify)) streamSettings.RealitySettings.Mldsa65Verify = mldsa65Verify;
                }
                else if (transport == Transport.TCP)
                {
                    streamSettings.TcpSettings ??= new();

                    // Header: Type
                    string type = kv.GetValueOrDefault("headertype", ConfigOutbound.OutboundStreamSettings.StreamTcpSettings.TcpHeader.Get.Type.None);
                    type = type.ToLower();
                    
                    streamSettings.TcpSettings.Header.Type = type;

                    if (type.Equals(ConfigOutbound.OutboundStreamSettings.StreamTcpSettings.TcpHeader.Get.Type.Http, StringComparison.OrdinalIgnoreCase))
                    {
                        // Request
                        streamSettings.TcpSettings.Header.Request = new()
                        {
                            // Header: Request: Version
                            Version = "1.1",

                            // Header: Request: Method
                            Method = "GET"
                        };

                        // Header: Request: Path
                        string path = kv.GetValueOrDefault("path", "/");
                        if (!string.IsNullOrEmpty(path)) streamSettings.TcpSettings.Header.Request.Path.Add(path);

                        // Header: Request: Headers
                        string host = kv.GetValueOrDefault("host", urid.Host);
                        if (!string.IsNullOrEmpty(host)) streamSettings.TcpSettings.Header.Request.Headers.Host.Add(host);
                        string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:144.0) Gecko/20100101 Firefox/144.0";
                        streamSettings.TcpSettings.Header.Request.Headers.UserAgent.Add(ua);
                        streamSettings.TcpSettings.Header.Request.Headers.AcceptEncoding.Add("gzip, deflate");
                        streamSettings.TcpSettings.Header.Request.Headers.Connection.Add("keep-alive");
                        streamSettings.TcpSettings.Header.Request.Headers.Pragma = "no-cache";

                        // Response
                        streamSettings.TcpSettings.Header.Response = new();
                    }
                }
                else if (transport == Transport.H2)
                {
                    // H2 (HTTP) Is Not Supported By Xray
                    // Last Support: v24.11.30

                    streamSettings.HttpSettings ??= new();

                    // Host
                    List<string> hostList = new();
                    string hosts = kv.GetValueOrDefault("host", urid.Host);
                    if (hosts.Contains(','))
                    {
                        hostList = hosts.Split(',').ToList();
                    }
                    else hostList.Add(hosts);
                    streamSettings.HttpSettings.Host = hostList;

                    // Path
                    string path = kv.GetValueOrDefault("path", "/");
                    if (!string.IsNullOrEmpty(path)) streamSettings.HttpSettings.Path = path;
                }
                else if (transport == Transport.XHTTP)
                {
                    streamSettings.XHttpSettings ??= new();

                    // Host
                    string host = kv.GetValueOrDefault("host", urid.Host);
                    if (!string.IsNullOrEmpty(host)) streamSettings.XHttpSettings.Host = host;

                    // Path
                    string path = kv.GetValueOrDefault("path", "/");
                    if (!string.IsNullOrEmpty(path)) streamSettings.XHttpSettings.Path = path;

                    // Mode
                    string mode = kv.GetValueOrDefault("mode", ConfigOutbound.OutboundStreamSettings.StreamXHttpSettings.Get.Mode.Auto);
                    streamSettings.XHttpSettings.Mode = mode;

                    // Extra JSON
                    string extraJson = kv.GetValueOrDefault("extra", string.Empty);
                    if (!string.IsNullOrEmpty(extraJson))
                    {
                        bool isJsonValid = JsonTool.IsValid(extraJson);
                        if (isJsonValid)
                        {
                            streamSettings.XHttpSettings.Extra = JsonDocument.Parse(extraJson);
                        }
                    }
                }
                else if (transport == Transport.Quic)
                {
                    // Quic Is Not Supported By Xray
                    // Last Support: v1.8.24

                    streamSettings.QuicSettings ??= new();

                    // Security
                    streamSettings.QuicSettings.Security = kv.GetValueOrDefault("quicsecurity", ConfigOutbound.OutboundStreamSettings.StreamQuicSettings.Get.Security.None);

                    // Key
                    string key = kv.GetValueOrDefault("key", string.Empty);
                    if (!string.IsNullOrEmpty(key)) streamSettings.QuicSettings.Key = key;

                    // Header: Type
                    streamSettings.QuicSettings.Header.Type = kv.GetValueOrDefault("headertype", ConfigOutbound.OutboundStreamSettings.StreamQuicSettings.QuicHeader.Get.Type.None);
                }
                else if (transport == Transport.KCP)
                {
                    streamSettings.KcpSettings ??= new();

                    // Header: Domain
                    string host = kv.GetValueOrDefault("host", string.Empty);
                    if (!string.IsNullOrEmpty(host)) streamSettings.KcpSettings.Header.Domain = host;

                    // Header: Type
                    streamSettings.KcpSettings.Header.Type = kv.GetValueOrDefault("headertype", ConfigOutbound.OutboundStreamSettings.StreamKcpSettings.KcpHeader.Get.Type.None);

                    // Seed
                    string seed = kv.GetValueOrDefault("seed", string.Empty);
                    if (!string.IsNullOrEmpty(seed)) streamSettings.KcpSettings.Seed = seed;
                }
                else if (transport == Transport.GRPC)
                {
                    // gRPC With GUN Is Not Supported By Xray
                    // Last Support: v24.9.30
                    // Is Getting Deprecated In Xray

                    streamSettings.GrpcSettings ??= new();

                    // Mode
                    string mode = kv.GetValueOrDefault("mode", string.Empty).Trim();
                    streamSettings.GrpcSettings.MultiMode = mode.Equals("multi", StringComparison.OrdinalIgnoreCase);

                    // Authority
                    string auth = kv.GetValueOrDefault("authority", kv.GetValueOrDefault("auth", string.Empty));
                    if (!string.IsNullOrEmpty(auth)) streamSettings.GrpcSettings.Authority = auth;

                    // ServiceName
                    string serviceName = kv.GetValueOrDefault("servicename", string.Empty);
                    if (!string.IsNullOrEmpty(serviceName)) streamSettings.GrpcSettings.ServiceName = serviceName;

                    // User_agent
                    string userAgent = kv.GetValueOrDefault("useragent", string.Empty);
                    if (!string.IsNullOrEmpty(userAgent)) streamSettings.GrpcSettings.User_agent = userAgent;
                }
                else if (transport == Transport.WS)
                {
                    // Is Getting Deprecated In Xray

                    streamSettings.WsSettings ??= new();

                    // Path
                    string path = kv.GetValueOrDefault("path", "/");
                    if (!string.IsNullOrEmpty(path)) streamSettings.WsSettings.Path = path;

                    // Host
                    string host = kv.GetValueOrDefault("host", urid.Host);
                    if (!string.IsNullOrEmpty(host)) streamSettings.WsSettings.Host = host;

                    // Headers
                    //streamSettings.WsSettings.Headers.Add("Host", host); // Deprecated In Xray

                    // Alpn For WS Is Deprecated In Xray
                    //streamSettings.TlsSettings?.Alpn.Clear();
                }
                else if (transport == Transport.HttpUpgrade)
                {
                    streamSettings.HttpUpgradeSettings ??= new();

                    // Path
                    string path = kv.GetValueOrDefault("path", "/");
                    if (!string.IsNullOrEmpty(path)) streamSettings.HttpUpgradeSettings.Path = path;

                    // Host
                    string host = kv.GetValueOrDefault("host", urid.Host);
                    if (!string.IsNullOrEmpty(host)) streamSettings.HttpUpgradeSettings.Host = host;

                    // Headers
                    //streamSettings.HttpUpgradeSettings.Headers.Add("Host", host); // Deprecated In Xray
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConfigBuilder_Methods SetStreamSettings: " + ex.Message);
                throw;
            }

            return streamSettings;
        }
    }

    private static ConfigPolicy AddPolicies()
    {
        return new ConfigPolicy()
        {
            Levels = new()
            {
                // Add Policy Level 0 (Default)
                { "0", new() },
                // Add Policy Level 9
                {
                    "9",
                    new()
                    {
                        UplinkOnly = 1,
                        DownlinkOnly = 1
                    }
                }
            }
        };
    }

    private static ConfigDns AddDnsModule(IPAddress bootstrapIP, int bootstrapPort, bool addLoopbackDns, string? doh)
    {
        ConfigDns configDns = new()
        {
            Tag = "dns-module",

            Hosts = new()
            {
                {
                    "dns.google",
                    new()
                    {
                        "8.8.8.8",
                        "8.8.4.4",
                        "2001:4860:4860::8888",
                        "2001:4860:4860::8844"
                    }
                },
                {
                    "dns.cloudflare.com",
                    new()
                    {
                        "104.16.249.249",
                        "104.16.248.249",
                        "2606:4700::6810:f8f9",
                        "2606:4700::6810:f9f9"
                    }
                },
                { "youtube.com", new() { "google.com" } },
            },

            Servers = new()
            {
                new()
                {
                    Address = "https://dns.cloudflare.com/dns-query",
                    Port = 443
                },
                new()
                {
                    Address = bootstrapIP.ToStringNoScopeId(),
                    Port = bootstrapPort,
                    Domains = new()
                    {
                        //"geosite:private",
                        //"geosite:category-ir",
                        "full:dns.cloudflare.com"
                    }
                },
            }
        };

        try
        {
            // Add DoH To Top
            if (!string.IsNullOrEmpty(doh))
            {
                NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(doh, 443);
                ConfigDns.Server server = new()
                {
                    Address = doh,
                    Port = urid.Port
                };
                configDns.Servers.Insert(0, server);

                foreach (ConfigDns.Server dns in configDns.Servers)
                {
                    if (dns.Address.Equals(bootstrapIP.ToStringNoScopeId()))
                    {
                        string dohDomain = $"full:{urid.Host}";
                        dns.Domains.Add(dohDomain);
                        break;
                    }
                }
            }

            // Add Loopback DNS Server To Top
            if (addLoopbackDns)
            {
                ConfigDns.Server server = new()
                {
                    Address = IPAddress.Loopback.ToString(),
                    Port = 53
                };
                configDns.Servers.Insert(0, server);
            }
        }
        catch (Exception) { }

        return configDns;
    }

    private static ConfigInbound AddInbound_Dns(int listeningDnsPort, IPAddress bootstrapIP, int bootstrapPort)
    {
        return new ConfigInbound()
        {
            Tag = "dns-in",
            Listen = IPAddress.Any.ToString(),
            Port = listeningDnsPort, // 10853
            Protocol = ConfigInbound.Get.Protocol.Dokodemo_door,
            Settings = new DokoDemoDoorSettingsIn()
            {
                Address = bootstrapIP.ToStringNoScopeId(),
                Port = bootstrapPort,
                Network = DokoDemoDoorSettingsIn.Get.Network.TcpUdp,
                FollowRedirect = false,
                UserLevel = 0
            }
        };
    }

    private static ConfigInbound AddInbound_Mixed(int listeningMixedPort)
    {
        return new ConfigInbound()
        {
            Tag = "mixed-in",
            Listen = IPAddress.Any.ToString(),
            Port = listeningMixedPort, // 10808
            Protocol = ConfigInbound.Get.Protocol.Mixed,
            Settings = new Inbounds.MixedSettingsIn()
            {
                Udp = true,
                IP = IPAddress.Any.ToString()
            },
            Sniffing = new()
        };
    }

    private static ConfigInbound AddInbound_Socks(int listeningSocksPort)
    {
        return new ConfigInbound()
        {
            Tag = "socks-in",
            Listen = IPAddress.Any.ToString(),
            Port = listeningSocksPort, // 10808
            Protocol = ConfigInbound.Get.Protocol.Socks,
            Settings = new Inbounds.SocksSettingsIn()
            {
                Udp = true,
                IP = IPAddress.Any.ToString()
            },
            Sniffing = new()
        };
    }

    private static ConfigInbound AddInbound_Http(int listeningHttpPort)
    {
        return new ConfigInbound()
        {
            Tag = "http-in",
            Listen = IPAddress.Any.ToString(),
            Port = listeningHttpPort, // 10809
            Protocol = ConfigInbound.Get.Protocol.Http,
            Settings = new Inbounds.HttpSettingsIn(),
            Sniffing = new()
        };
    }

    private static ConfigOutbound AddOutbound_Direct()
    {
        return new ConfigOutbound()
        {
            Tag = "direct-out",
            Protocol = ConfigOutbound.Get.Protocol.Freedom,
            Settings = new FreedomSettings()
            {
                DomainStrategy = FreedomSettings.Get.DomainStrategy.UseIP,
                Fragment = null
            },
            StreamSettings = new()
            {
                Sockopt = new()
                {
                    DomainStrategy = ConfigOutbound.OutboundStreamSettings.StreamSockopt.Get.DomainStrategy.UseIP
                }
            }
        };
    }

    private static ConfigOutbound AddOutbound_Block()
    {
        return new ConfigOutbound()
        {
            Tag = "block-out",
            Protocol = ConfigOutbound.Get.Protocol.Blackhole,
            Settings = new BlackholeSettings()
        };
    }

    private static ConfigOutbound AddOutbound_Dns()
    {
        // If Fragment Is Active dialerProxy Must Be Set
        return new ConfigOutbound()
        {
            Tag = "dns-out",
            Protocol = ConfigOutbound.Get.Protocol.Dns,
            //Settings = new DnsSettings()
            //{
            //    Network = DnsSettings.Get.Network.Tcp,
            //    Address = "8.8.8.8",
            //    Port = 53,
            //    NonIPQuery = DnsSettings.Get.NonIPQuery.Skip
            //}
        };
    }

    /// <summary>
    /// Add Freedom Outbound
    /// </summary>
    /// <param name="xrayConfig">Xray Config</param>
    /// <param name="fragment">Fragment Parameters e.g. "tlshello,2-4,10-11"</param>
    /// <param name="noise">Noise Parameters</param>
    private static ConfigOutbound? AddOutbound_Freedom(string tag, string? fragment, string? noise)
    {
        try
        {
            // Fragment
            if (!string.IsNullOrEmpty(fragment) || !string.IsNullOrEmpty(noise))
            {
                ConfigOutbound outbound_Freedom = new()
                {
                    Tag = tag,
                    Protocol = ConfigOutbound.Get.Protocol.Freedom,
                    StreamSettings = new()
                    {
                        Sockopt = new()
                        {
                            Mark = 255,
                            DomainStrategy = ConfigOutbound.OutboundStreamSettings.StreamSockopt.Get.DomainStrategy.UseIP,
                            TcpKeepAliveIdle = 100,
                            TcpNoDelay = true
                        }
                    }
                };

                FreedomSettings freedomSettings = new()
                {
                    DomainStrategy = FreedomSettings.Get.DomainStrategy.UseIP,
                    UserLevel = 9
                };

                // Fragment
                if (!string.IsNullOrEmpty(fragment))
                {
                    // Get Packets, Length, Interval
                    string packets = fragment;
                    string lenght = string.Empty, interval = string.Empty;
                    if (packets.Contains(','))
                    {
                        string[] split = packets.Split(',', StringSplitOptions.TrimEntries);
                        if (split.Length > 0)
                        {
                            packets = split[0];
                            if (!packets.Contains('-')) packets = "tlshello";
                        }
                        if (split.Length > 1) lenght = split[1];
                        if (split.Length > 2) interval = split[2];
                    }

                    freedomSettings.Fragment = new()
                    {
                        Packets = packets,
                        Length = !string.IsNullOrEmpty(lenght) ? lenght : "2-4",
                        Interval = !string.IsNullOrEmpty(interval) ? interval : "3-5",
                    };
                }

                // Noise e.g. Count,Packet,Delay 20,50-150,10-11
                if (!string.IsNullOrEmpty(noise))
                {
                    int count = 0;
                    string countStr = "0";
                    string packet = string.Empty, delay = string.Empty;
                    if (noise.Contains(','))
                    {
                        string[] split = noise.Split(',', StringSplitOptions.TrimEntries);
                        if (split.Length > 0)
                        {
                            countStr = split[0];
                            bool hasMinMax = false;
                            if (countStr.Contains('-'))
                            {
                                string[] splitC = noise.Split(',', StringSplitOptions.TrimEntries);
                                if (splitC.Length > 1)
                                {
                                    string minStr = splitC[0];
                                    string maxStr = splitC[1];
                                    bool isIntMin = int.TryParse(minStr, out int minCount);
                                    bool isIntMax = int.TryParse(maxStr, out int maxCount);
                                    if (isIntMin && isIntMax)
                                    {
                                        Random random = new();
                                        count = random.Next(minCount, maxCount);
                                        hasMinMax = true;
                                    }
                                }
                            }
                            if (!hasMinMax)
                            {
                                bool isInt = int.TryParse(countStr, out int countOut);
                                if (isInt) count = countOut;
                            }
                        }
                        if (split.Length > 1) packet = split[1];
                        if (split.Length > 2) delay = split[2];
                    }

                    if (noise.Equals("IPv4", StringComparison.OrdinalIgnoreCase))
                    {
                        count = 24;
                        packet = "1250";
                        delay = "10";
                    }
                    else if (noise.Equals("IPv6", StringComparison.OrdinalIgnoreCase))
                    {
                        count = 24;
                        packet = "1230";
                        delay = "10";
                    }

                    for (int n = 0; n < count; n++)
                    {
                        FreedomSettings.Noise noise0 = new()
                        {
                            Type = FreedomSettings.Noise.Get.Type.Rand,
                            Packet = !string.IsNullOrEmpty(packet) ? packet : "50-150",
                            Delay = !string.IsNullOrEmpty(delay) ? delay : "10-11"
                        };
                        freedomSettings.Noises.Add(noise0);
                    }

                    if (count > 0 && string.IsNullOrEmpty(fragment))
                    {
                        // Remove Default Fragment Values
                        freedomSettings.Fragment = null;
                    }
                }

                outbound_Freedom.Settings = freedomSettings;
                return outbound_Freedom;
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ConfigBuilder AddFreedomOutbound: " + ex.Message);
            return null;
        }
    }

    private static ConfigOutbound AddOutbound_FakeVlessForOlderCores()
    {
        // If Fragment Is Active dialerProxy Must Be Set
        return new ConfigOutbound()
        {
            Tag = "fake-proxy-out",
            Protocol = ConfigOutbound.Get.Protocol.Vless,
            Settings = new VlessSettings()
            {
                Vnext = new()
                {
                    new()
                    {
                        Address = "example.com",
                        Port = 443,
                        Users = new()
                        {
                            new()
                            {
                                ID = "UUID",
                                Encryption = "none"
                            }
                        }
                    }
                }
            }
        };
    }

}