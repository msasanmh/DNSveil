using System;
using System.Diagnostics;
using System.Net;
using MsmhToolsClass.ProxifiedClients;
using MsmhToolsClass.V2RayConfigTool.Inbounds;
using MsmhToolsClass.V2RayConfigTool.Outbounds;
using Newtonsoft.Json.Linq;
using static MsmhToolsClass.V2RayConfigTool.XrayConfig;

namespace MsmhToolsClass.V2RayConfigTool;

public partial class ConfigBuilder
{
    public enum Protocol
    {
        Unknown,
        Vless,
        Vmess,
        ShadowSocks,
        Trojan,
        WireGuard,
        HTTP,
        SOCKS,
        Hysteria, // Not Supported By Xray
        Hysteria2 // Not Supported By Xray
    }

    private static Protocol GetProtocolByUrl(string url)
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

    private static ConfigOutbound.OutboundStreamSettings SetStreamSettings(ConfigOutbound.OutboundStreamSettings streamSettings, Dictionary<string, string> kv, NetworkTool.URL urid)
    {
        // Modify Based On Transport Layer
        streamSettings = setStreamSettingsInternal(streamSettings.Network, kv, urid, streamSettings);
        // Modify Based On Security
        streamSettings = setStreamSettingsInternal(streamSettings.Security, kv, urid, streamSettings);
        return streamSettings;

        static ConfigOutbound.OutboundStreamSettings setStreamSettingsInternal(string netwotkOrSecurity, Dictionary<string, string> kv, NetworkTool.URL urid, ConfigOutbound.OutboundStreamSettings streamSettings)
        {
            try
            {
                if (netwotkOrSecurity.Equals("reality", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.RealitySettings ??= new();

                    // Fingerprint
                    streamSettings.RealitySettings.Fingerprint = streamSettings.TlsSettings.Fingerprint;

                    // ServerName
                    streamSettings.RealitySettings.ServerName = kv.GetValueOrDefault("sni", string.Empty);

                    // PublicKey
                    streamSettings.RealitySettings.PublicKey = kv.GetValueOrDefault("pbk", string.Empty);

                    // ShortId
                    streamSettings.RealitySettings.ShortId = kv.GetValueOrDefault("sid", string.Empty);

                    // SpiderX
                    streamSettings.RealitySettings.SpiderX = kv.GetValueOrDefault("spx", string.Empty);
                }
                else if (netwotkOrSecurity.Equals("raw", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.RawSettings ??= new();

                }
                else if (netwotkOrSecurity.Equals("xhttp", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.XHttpSettings ??= new();

                    // Host
                    streamSettings.XHttpSettings.Host = kv.GetValueOrDefault("host", urid.Host);

                    // Path
                    string path = kv.GetValueOrDefault("path", string.Empty);
                    if (!string.IsNullOrEmpty(path)) streamSettings.XHttpSettings.Path = path;

                    // Mode
                    string mode = kv.GetValueOrDefault("mode", string.Empty);
                    if (!string.IsNullOrEmpty(mode)) streamSettings.XHttpSettings.Mode = mode;

                    // Extra: headers
                    string header = kv.GetValueOrDefault("headers", string.Empty);
                    string[] headerKVP = header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string kvp in headerKVP)
                    {
                        string[] headers = kvp.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (headers.Length > 1)
                        {
                            streamSettings.XHttpSettings.Extra.Headers.Add(headers[0], headers[1]);
                        }
                    }
                    
                    // Extra: xPaddingBytes
                    string xPaddingBytes = kv.GetValueOrDefault("xPaddingBytes", string.Empty);
                    if (!string.IsNullOrEmpty(xPaddingBytes)) streamSettings.XHttpSettings.Extra.XPaddingBytes = xPaddingBytes;

                    // Extra: noGRPCHeader
                    string noGRPCHeader = kv.GetValueOrDefault("noGRPCHeader", string.Empty);
                    if (noGRPCHeader.Equals("1") || noGRPCHeader.Equals("true", StringComparison.OrdinalIgnoreCase)) streamSettings.XHttpSettings.Extra.NoGRPCHeader = true;

                    // Extra: noSSEHeader
                    string noSSEHeader = kv.GetValueOrDefault("noSSEHeader", string.Empty);
                    if (noSSEHeader.Equals("1") || noSSEHeader.Equals("true", StringComparison.OrdinalIgnoreCase)) streamSettings.XHttpSettings.Extra.NoSSEHeader = true;

                    // Extra: scMaxEachPostBytes
                    string scMaxEachPostBytes = kv.GetValueOrDefault("scMaxEachPostBytes", string.Empty);
                    bool isInt = int.TryParse(scMaxEachPostBytes, out int intValue);
                    if (isInt) streamSettings.XHttpSettings.Extra.ScMaxEachPostBytes = intValue;

                    // Extra: scMinPostsIntervalMs
                    string scMinPostsIntervalMs = kv.GetValueOrDefault("scMinPostsIntervalMs", string.Empty);
                    isInt = int.TryParse(scMinPostsIntervalMs, out intValue);
                    if (isInt) streamSettings.XHttpSettings.Extra.ScMinPostsIntervalMs = intValue;

                    // Extra: scMaxBufferedPosts
                    string scMaxBufferedPosts = kv.GetValueOrDefault("scMaxBufferedPosts", string.Empty);
                    isInt = int.TryParse(scMaxBufferedPosts, out intValue);
                    if (isInt) streamSettings.XHttpSettings.Extra.ScMaxBufferedPosts = intValue;

                    // Extra: scStreamUpServerSecs
                    string scStreamUpServerSecs = kv.GetValueOrDefault("scStreamUpServerSecs", string.Empty);
                    if (!string.IsNullOrEmpty(scStreamUpServerSecs)) streamSettings.XHttpSettings.Extra.ScStreamUpServerSecs = scStreamUpServerSecs;

                    // Extra: XMux: maxConcurrency
                    string maxConcurrency = kv.GetValueOrDefault("maxConcurrency", string.Empty);
                    if (!string.IsNullOrEmpty(maxConcurrency)) streamSettings.XHttpSettings.Extra.Xmux.MaxConcurrency = maxConcurrency;

                    // Extra: XMux: maxConnections
                    string maxConnections = kv.GetValueOrDefault("maxConnections", string.Empty);
                    isInt = int.TryParse(maxConnections, out intValue);
                    if (isInt) streamSettings.XHttpSettings.Extra.Xmux.MaxConnections = intValue;

                    // Extra: XMux: cMaxReuseTimes
                    string cMaxReuseTimes = kv.GetValueOrDefault("cMaxReuseTimes", string.Empty);
                    isInt = int.TryParse(cMaxReuseTimes, out intValue);
                    if (isInt) streamSettings.XHttpSettings.Extra.Xmux.CMaxReuseTimes = intValue;

                    // Extra: XMux: hMaxRequestTimes
                    string hMaxRequestTimes = kv.GetValueOrDefault("hMaxRequestTimes", string.Empty);
                    if (!string.IsNullOrEmpty(hMaxRequestTimes)) streamSettings.XHttpSettings.Extra.Xmux.HMaxRequestTimes = hMaxRequestTimes;

                    // Extra: XMux: hMaxReusableSecs
                    string hMaxReusableSecs = kv.GetValueOrDefault("hMaxReusableSecs", string.Empty);
                    if (!string.IsNullOrEmpty(hMaxReusableSecs)) streamSettings.XHttpSettings.Extra.Xmux.HMaxReusableSecs = hMaxReusableSecs;

                    // Extra: XMux: hKeepAlivePeriod
                    string hKeepAlivePeriod = kv.GetValueOrDefault("hKeepAlivePeriod", string.Empty);
                    isInt = int.TryParse(hKeepAlivePeriod, out intValue);
                    if (isInt) streamSettings.XHttpSettings.Extra.Xmux.HKeepAlivePeriod = intValue;
                }
                else if (netwotkOrSecurity.Equals("h2", StringComparison.OrdinalIgnoreCase))
                {
                    //item.Network = nameof(ETransport.h2);
                    //item.RequestHost = Utils.UrlDecode(query["host"] ?? "");
                    //item.Path = Utils.UrlDecode(query["path"] ?? "/");

                }
                else if (netwotkOrSecurity.Equals("quic", StringComparison.OrdinalIgnoreCase))
                {
                    //item.HeaderType = query["headerType"] ?? Global.None;
                    //item.RequestHost = query["quicSecurity"] ?? Global.None;
                    //item.Path = Utils.UrlDecode(query["key"] ?? "");

                }
                else if (netwotkOrSecurity.Equals("kcp", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.KcpSettings ??= new();

                    // Header: Type
                    streamSettings.KcpSettings.Header.Type = kv.GetValueOrDefault("headerType", ConfigOutbound.OutboundStreamSettings.StreamKcpSettings.KcpHeader.Get.Type.None);

                    // Seed
                    string seed = kv.GetValueOrDefault("seed", string.Empty);
                    if (!string.IsNullOrEmpty(seed)) streamSettings.KcpSettings.Seed = seed;
                }
                else if (netwotkOrSecurity.Equals("grpc", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.GrpcSettings ??= new();

                    // Authority
                    string auth = kv.GetValueOrDefault("authority", kv.GetValueOrDefault("auth", string.Empty));
                    if (!string.IsNullOrEmpty(auth)) streamSettings.GrpcSettings.Authority = auth;

                    // ServiceName
                    string serviceName = kv.GetValueOrDefault("serviceName", string.Empty);
                    if (!string.IsNullOrEmpty(serviceName)) streamSettings.GrpcSettings.ServiceName = serviceName;

                    // User_agent
                    string userAgent = kv.GetValueOrDefault("userAgent", string.Empty);
                    if (!string.IsNullOrEmpty(userAgent)) streamSettings.GrpcSettings.User_agent = userAgent;
                }
                else if (netwotkOrSecurity.Equals("ws", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.WsSettings ??= new();

                    // Path
                    streamSettings.WsSettings.Path = kv.GetValueOrDefault("path", "/");

                    // Host
                    string host = kv.GetValueOrDefault("host", urid.Host);
                    streamSettings.WsSettings.Host = host;

                    // Headers
                    //streamSettings.WsSettings.Headers.Add("Host", host); // Deprecated In Xray
                }
                else if (netwotkOrSecurity.Equals("httpupgrade", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.HttpUpgradeSettings ??= new();

                    // Path
                    streamSettings.HttpUpgradeSettings.Path = kv.GetValueOrDefault("path", "/");

                    // Host
                    string host = kv.GetValueOrDefault("host", urid.Host);
                    streamSettings.HttpUpgradeSettings.Host = host;

                    // Headers
                    //streamSettings.HttpUpgradeSettings.Headers.Add("Host", host); // Deprecated In Xray
                }
                else if (netwotkOrSecurity.Equals("tcp", StringComparison.OrdinalIgnoreCase))
                {
                    streamSettings.TcpSettings ??= new();

                    // Header: Request: Headers
                    streamSettings.TcpSettings.Header.Request.Headers.Add("Connection", "keep-alive");
                    streamSettings.TcpSettings.Header.Request.Headers.Add("Host", kv.GetValueOrDefault("host", urid.Host));

                    // Header: Request: Path
                    streamSettings.TcpSettings.Header.Request.Path.Add(kv.GetValueOrDefault("path", "/"));

                    // Header: Type
                    streamSettings.TcpSettings.Header.Type = kv.GetValueOrDefault("headerType", ConfigOutbound.OutboundStreamSettings.StreamTcpSettings.TcpHeader.Get.Type.None);
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

    private static ConfigDns AddBuiltInDns()
    {
        return new ConfigDns()
        {
            Hosts = new()
            {
                { "dns.google", new() { "8.8.8.8", "8.8.4.4" } },
                { "dns.cloudflare.com", new() { "cloudflare.com" } },
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
                    Address = "tcp://8.8.8.8",
                    Port = 53
                },
                new()
                {
                    Address = "tcp://9.9.9.9",
                    Port = 53
                },
                new()
                {
                    Address = "localhost",
                    Port = 53,
                    Domains = new()
                    {
                        //"geosite:private",
                        //"geosite:category-ir",
                        "full:cloudflare.com"
                    }
                },
                new()
                {
                    Address = "tcp://127.0.0.1",
                    Port = 53
                }
            }
        };
    }

    private static ConfigInbound AddInbound_Dns()
    {
        return new ConfigInbound()
        {
            Tag = "dns-in",
            Listen = IPAddress.Any.ToString(),
            Port = 10853,
            Protocol = ConfigInbound.Get.Protocol.Dokodemo_door,
            Settings = new DokoDemoDoorSettings()
            {
                Address = "8.8.8.8",
                Port = 53,
                Network = DokoDemoDoorSettings.Get.Network.TcpUdp,
                FollowRedirect = false,
                UserLevel = 0
            }
        };
    }

    private static ConfigInbound AddInbound_Socks()
    {
        return new ConfigInbound()
        {
            Tag = "socks-in",
            Listen = IPAddress.Any.ToString(),
            Port = 10808,
            Protocol = ConfigInbound.Get.Protocol.Socks,
            Settings = new SocksSettings()
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
                DomainStrategy = FreedomSettings.Get.DomainStrategy.ForceIP,
                Fragment = null
            },
            StreamSettings = new()
            {
                Sockopt = new()
                {
                    DomainStrategy = ConfigOutbound.OutboundStreamSettings.StreamSockopt.Get.DomainStrategy.ForceIP
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
            Settings = new DnsSettings()
            {
                Network = DnsSettings.Get.Network.Tcp,
                Address = "8.8.8.8",
                Port = 53,
                NonIPQuery = DnsSettings.Get.NonIPQuery.Skip
            }
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