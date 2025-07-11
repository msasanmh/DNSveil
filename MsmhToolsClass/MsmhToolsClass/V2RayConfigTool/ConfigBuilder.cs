using System.Diagnostics;
using System.Net;
using System.Text;
using MsmhToolsClass.V2RayConfigTool.Inbounds;
using MsmhToolsClass.V2RayConfigTool.Outbounds;
using static MsmhToolsClass.V2RayConfigTool.XrayConfig;

namespace MsmhToolsClass.V2RayConfigTool;

public partial class ConfigBuilder
{
    public static XrayConfig Build(string url)
    {
        return Build_Internal(url);
    }

    private static XrayConfig Build_Internal(string url)
    {
        XrayConfig xrayConfig = new();

        try
        {
            url = url.Trim();
            if (EncodingTool.IsBase64String(url))
            {
                url = Encoding.UTF8.GetString(EncodingTool.Base64UrlDecode(url));
                url = url.Trim();
            }
            Debug.WriteLine(url);
            Protocol protocol = GetProtocolByUrl(url);
            if (protocol == Protocol.Unknown) return xrayConfig;

            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(url, 443);
            Dictionary<string, string> kv = NetworkTool.ParseUriQuery(urid.Query, true).ToDictionary();

            // Create Policies
            ConfigPolicy configPolicy = AddPolicies();

            // Create Built-In Dns
            ConfigDns configDns = AddBuiltInDns();

            // Create Dns Inbound
            ConfigInbound dns_in = AddInbound_Dns();

            // Create Socks Inbound
            ConfigInbound socks_in = AddInbound_Socks();

            // Create Direct Outbound
            ConfigOutbound direct_out = AddOutbound_Direct();

            // Create Block Outbound
            ConfigOutbound block_out = AddOutbound_Block();

            // Create DNS Outbound
            ConfigOutbound dns_out = AddOutbound_Dns();

            // Create Fragment Outbound
            string? fragment = kv.GetValueOrDefault("fragment");
            ConfigOutbound? fragment_out = AddOutbound_Freedom("fragment-out", fragment, noise: null);

            // Create Noise Outbound
            string? noise = kv.GetValueOrDefault("noise");
            ConfigOutbound? noise_out = null;
            ConfigOutbound? noiseIPv4_out = null;
            ConfigOutbound? noiseIPv6_out = null;
            if (!string.IsNullOrEmpty(noise))
            {
                if (noise.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                {
                    // Auto
                    noiseIPv4_out = AddOutbound_Freedom("noiseIPv4-out", fragment: null, "IPv4");
                    noiseIPv6_out = AddOutbound_Freedom("noiseIPv6-out", fragment: null, "IPv6");
                }
                else
                {
                    // Manual
                    noise_out = AddOutbound_Freedom("noise-out", fragment: null, noise);
                }
            }

            // Create Proxy Outbound
            ConfigOutbound proxy_out = new()
            {
                Tag = "proxy-out"
            };

            if (protocol == Protocol.Vless)
            {
                proxy_out.Protocol = ConfigOutbound.Get.Protocol.Vless;
                proxy_out.Settings = new VlessSettings()
                {
                    Vnext = new()
                    {
                        new()
                        {
                            Address = urid.Host,
                            Port = urid.Port,
                            Users = new()
                            {
                                new()
                                {
                                    ID = urid.Username,
                                    Encryption = kv.GetValueOrDefault("encryption", "none"),
                                    Flow = kv.GetValueOrDefault("flow"),
                                    Level = kv.GetValueOrDefault("level", "0").ToInt(0)
                                }
                            }
                        }
                    }
                };
            }
            else if (protocol == Protocol.Vmess)
            {

            }
            else if (protocol == Protocol.ShadowSocks)
            {

            }
            else if (protocol == Protocol.Trojan)
            {

            }
            else if (protocol == Protocol.WireGuard)
            {

            }
            else if (protocol == Protocol.HTTP)
            {

            }
            else if (protocol == Protocol.SOCKS)
            {

            }

            // Remarks
            string? remarks = kv.GetValueOrDefault("remarks");
            if (!string.IsNullOrEmpty(remarks)) xrayConfig.Remarks = remarks;

            // StreamSettings
            // StreamSettings: Network
            string? netwotkType = kv.GetValueOrDefault("type");
            if (string.IsNullOrEmpty(netwotkType)) netwotkType = kv.GetValueOrDefault("network");
            if (string.IsNullOrEmpty(netwotkType)) netwotkType = ConfigOutbound.OutboundStreamSettings.Get.Network.Raw;
            proxy_out.StreamSettings.Network = netwotkType;

            // StreamSettings: Security
            proxy_out.StreamSettings.Security = kv.GetValueOrDefault("security", ConfigOutbound.OutboundStreamSettings.Get.Security.None);

            // StreamSettings: TlsSettings: ServerName
            proxy_out.StreamSettings.TlsSettings.ServerName = kv.GetValueOrDefault("host", kv.GetValueOrDefault("sni", urid.Host));

            // StreamSettings: TlsSettings: AllowInsecure
            string allowInsecure = kv.GetValueOrDefault("allowInsecure", string.Empty);
            proxy_out.StreamSettings.TlsSettings.AllowInsecure = allowInsecure.Equals("1") || allowInsecure.Equals("true", StringComparison.OrdinalIgnoreCase);

            // StreamSettings: TlsSettings: Alpn
            string? alpns = kv.GetValueOrDefault("alpn");
            if (!string.IsNullOrEmpty(alpns))
            {
                List<string> alpn = new();
                if (alpns.Contains(','))
                {
                    alpn = alpns.Split(',').ToList();
                }
                else alpn.Add(alpns);
                proxy_out.StreamSettings.TlsSettings.Alpn = alpn;
            }

            // StreamSettings: TlsSettings: Fingerprint
            string fingerprint = kv.GetValueOrDefault("fp", kv.GetValueOrDefault("fingerprint", ConfigOutbound.OutboundStreamSettings.StreamTlsSettings.Get.Fingerprint.Chrome));
            proxy_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;

            // Modify Based On Transport Layer And Security
            proxy_out.StreamSettings = SetStreamSettings(proxy_out.StreamSettings, kv, urid);

            // Add
            xrayConfig.Policy = configPolicy;
            xrayConfig.Dns = configDns;
            xrayConfig.Inbounds.Add(dns_in);
            xrayConfig.Inbounds.Add(socks_in);
            xrayConfig.Outbounds.Add(direct_out); // Must Be The First Outbound (V2rayNG)
            xrayConfig.Outbounds.Add(block_out);

            if (fragment_out != null)
            {
                xrayConfig.Outbounds.Add(fragment_out);
                dns_out.StreamSettings.Sockopt.DialerProxy = fragment_out.Tag;
                proxy_out.StreamSettings.Sockopt.DialerProxy = fragment_out.Tag;
            }

            if (noise_out != null) xrayConfig.Outbounds.Add(noise_out);
            if (noiseIPv4_out != null) xrayConfig.Outbounds.Add(noiseIPv4_out);
            if (noiseIPv6_out != null) xrayConfig.Outbounds.Add(noiseIPv6_out);

            xrayConfig.Outbounds.Add(dns_out);

            // Has Built-In DNS Server?
            bool hasBuiltInDns = xrayConfig.Dns.Servers.Count > 0;
            if (hasBuiltInDns)
            {
                proxy_out.StreamSettings.Sockopt.DomainStrategy = ConfigOutbound.OutboundStreamSettings.StreamSockopt.Get.DomainStrategy.UseIP;
            }

            if (fragment_out != null || noise_out != null || noiseIPv4_out != null || noiseIPv6_out != null)
            {
                proxy_out.StreamSettings.Sockopt.Mark = 255;
                proxy_out.StreamSettings.Sockopt.TcpKeepAliveIdle = 100;
                proxy_out.StreamSettings.Sockopt.TcpNoDelay = true;
            }

            // Add Proxy Outbound
            xrayConfig.Outbounds.Add(proxy_out);

            // Add geosite/geoip? Not All Clients Are Compatible When There's No .dat File.
            bool addGeo = false;

            // Create Routing Rules
            ConfigRouting.Rule rule1 = new()
            {
                InboundTag = new()
                {
                    dns_in.Tag
                },
                OutboundTag = dns_out.Tag
            };

            ConfigRouting.Rule rule2 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = dns_out.Tag,
                Port = "53"
            };

            ConfigRouting.Rule rule3 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = block_out.Tag,
                Network = ConfigRouting.Rule.Get.Network.TcpUdp,
                Domain = new()
                {
                    "geosite:category-ads-all"
                }
            };

            ConfigRouting.Rule rule4 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = direct_out.Tag,
                Network = ConfigRouting.Rule.Get.Network.TcpUdp,
                Domain = new()
                {
                    "geosite:private",
                    "geosite:category-ir"
                },
                IP = new()
                {
                    "geoip:private",
                    "geoip:ir"
                }
            };

            bool hasNoise = noise_out != null || noiseIPv4_out != null || noiseIPv6_out != null;
            ConfigRouting.Rule rule101 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = proxy_out.Tag,
                Network = hasNoise ? ConfigRouting.Rule.Get.Network.Tcp : ConfigRouting.Rule.Get.Network.TcpUdp
            };

            // Add Routing Rules
            xrayConfig.Routing.Rules.Add(rule1);
            xrayConfig.Routing.Rules.Add(rule2);
            if (addGeo)
            {
                xrayConfig.Routing.Rules.Add(rule3);
                xrayConfig.Routing.Rules.Add(rule4);
            }
            xrayConfig.Routing.Rules.Add(rule101);

            if (hasNoise)
            {
                if (noise_out != null)
                {
                    ConfigOutbound? proxy_udp_out = proxy_out.Clone();
                    if (proxy_udp_out != null)
                    {
                        proxy_udp_out.Tag = "proxy-udp-out";
                        proxy_udp_out.StreamSettings.Sockopt.DialerProxy = noise_out.Tag;
                        xrayConfig.Outbounds.Add(proxy_udp_out);
                        ConfigRouting.Rule rule102 = new()
                        {
                            InboundTag = new()
                            {
                                socks_in.Tag
                            },
                            OutboundTag = proxy_udp_out.Tag,
                            Network = ConfigRouting.Rule.Get.Network.Udp
                        };
                        xrayConfig.Routing.Rules.Add(rule102);
                    }
                }
                else
                {
                    if (noiseIPv4_out != null)
                    {
                        ConfigOutbound? proxy_udpIPv4_out = proxy_out.Clone();
                        if (proxy_udpIPv4_out != null)
                        {
                            proxy_udpIPv4_out.Tag = "proxy-udpIPv4-out";
                            proxy_udpIPv4_out.StreamSettings.Sockopt.DialerProxy = noiseIPv4_out.Tag;
                            xrayConfig.Outbounds.Add(proxy_udpIPv4_out);
                            ConfigRouting.Rule rule102 = new()
                            {
                                InboundTag = new()
                                {
                                    socks_in.Tag
                                },
                                OutboundTag = proxy_udpIPv4_out.Tag,
                                Network = ConfigRouting.Rule.Get.Network.Udp,
                                IP = new()
                                {
                                    "0.0.0.0/0"
                                }
                            };
                            xrayConfig.Routing.Rules.Add(rule102);
                        }
                    }

                    if (noiseIPv6_out != null)
                    {
                        ConfigOutbound? proxy_udpIPv6_out = proxy_out.Clone();
                        if (proxy_udpIPv6_out != null)
                        {
                            proxy_udpIPv6_out.Tag = "proxy-udpIPv6-out";
                            proxy_udpIPv6_out.StreamSettings.Sockopt.DialerProxy = noiseIPv6_out.Tag;
                            xrayConfig.Outbounds.Add(proxy_udpIPv6_out);
                            ConfigRouting.Rule rule102 = new()
                            {
                                InboundTag = new()
                                {
                                    socks_in.Tag
                                },
                                OutboundTag = proxy_udpIPv6_out.Tag,
                                Network = ConfigRouting.Rule.Get.Network.Udp,
                                IP = new()
                                {
                                    "::/0"
                                }
                            };
                            xrayConfig.Routing.Rules.Add(rule102);
                        }
                    }
                }
            }

            xrayConfig.Log.DnsLog = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ConfigBuilder Build_Internal: " + ex.Message);
        }

        return xrayConfig;
    }

    public static XrayConfig Build_Serverless(string? fragment = "tlshello,2-4,3-5", string? noise = "Auto")
    {
        string url = $"free://example.com/?fragment={fragment}&noise={noise}&fp=firefox";
        return Build_Serverless_Internal(url);
    }

    private static XrayConfig Build_Serverless_Internal(string url)
    {
        XrayConfig xrayConfig = new();

        try
        {
            url = url.Trim();
            
            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(url, 443);
            Dictionary<string, string> kv = NetworkTool.ParseUriQuery(urid.Query, true).ToDictionary();

            // Create Policies
            ConfigPolicy configPolicy = AddPolicies();

            // Create Built-In Dns
            ConfigDns configDns = AddBuiltInDns();

            // Create Dns Inbound
            ConfigInbound dns_in = AddInbound_Dns();

            // Create Socks Inbound
            ConfigInbound socks_in = AddInbound_Socks();

            // Create Direct Outbound
            ConfigOutbound direct_out = AddOutbound_Direct();

            // Create Block Outbound
            ConfigOutbound block_out = AddOutbound_Block();

            // Create DNS Outbound
            ConfigOutbound dns_out = AddOutbound_Dns();

            // Create Fragment Outbound
            string? fragment = kv.GetValueOrDefault("fragment");
            ConfigOutbound? fragment_out = AddOutbound_Freedom("fragment-out", fragment, noise: null);
            if (fragment_out == null)
            {
                Debug.WriteLine("ConfigBuilder Build_Serverless_Internal: fragment_out Is NULL.");
                return xrayConfig;
            }

            // Create Noise Outbound
            string? noise = kv.GetValueOrDefault("noise");
            ConfigOutbound? noise_out = null;
            ConfigOutbound? noiseIPv4_out = null;
            ConfigOutbound? noiseIPv6_out = null;
            if (!string.IsNullOrEmpty(noise))
            {
                if (noise.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                {
                    // Auto
                    noiseIPv4_out = AddOutbound_Freedom("noiseIPv4-out", fragment: null, "IPv4");
                    noiseIPv6_out = AddOutbound_Freedom("noiseIPv6-out", fragment: null, "IPv6");
                }
                else
                {
                    // Manual
                    noise_out = AddOutbound_Freedom("noise-out", fragment: null, noise);
                }
            }

            // Remarks
            string? remarks = kv.GetValueOrDefault("remarks");
            if (!string.IsNullOrEmpty(remarks)) xrayConfig.Remarks = remarks;

            // StreamSettings: TlsSettings: Fingerprint
            string fingerprint = kv.GetValueOrDefault("fp", kv.GetValueOrDefault("fingerprint", ConfigOutbound.OutboundStreamSettings.StreamTlsSettings.Get.Fingerprint.Chrome));
            
            // Add
            xrayConfig.Policy = configPolicy;
            xrayConfig.Dns = configDns;
            xrayConfig.Inbounds.Add(dns_in);
            xrayConfig.Inbounds.Add(socks_in);
            xrayConfig.Outbounds.Add(direct_out); // Must Be The First Outbound (V2rayNG)
            xrayConfig.Outbounds.Add(block_out);

            fragment_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
            xrayConfig.Outbounds.Add(fragment_out);
            dns_out.StreamSettings.Sockopt.DialerProxy = fragment_out.Tag;

            if (noise_out != null)
            {
                noise_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
                xrayConfig.Outbounds.Add(noise_out);
            }

            if (noiseIPv4_out != null)
            {
                noiseIPv4_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
                xrayConfig.Outbounds.Add(noiseIPv4_out);
            }

            if (noiseIPv6_out != null)
            {
                noiseIPv6_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
                xrayConfig.Outbounds.Add(noiseIPv6_out);
            }

            xrayConfig.Outbounds.Add(dns_out);

            bool addFakeProxyForOlderCores = false;
            if (addFakeProxyForOlderCores)
            {
                xrayConfig.Outbounds.Add(AddOutbound_FakeVlessForOlderCores());
            }

            // Add geosite/geoip? Not All Clients Are Compatible When There's No .dat File.
            bool addGeo = false;

            // Create Routing Rules
            ConfigRouting.Rule rule1 = new()
            {
                InboundTag = new()
                {
                    dns_in.Tag
                },
                OutboundTag = dns_out.Tag
            };

            ConfigRouting.Rule rule2 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = dns_out.Tag,
                Port = "53"
            };

            ConfigRouting.Rule rule3 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = block_out.Tag,
                Network = ConfigRouting.Rule.Get.Network.TcpUdp,
                Domain = new()
                {
                    "geosite:category-ads-all"
                }
            };

            ConfigRouting.Rule rule4 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = direct_out.Tag,
                Network = ConfigRouting.Rule.Get.Network.TcpUdp,
                Domain = new()
                {
                    "geosite:private",
                    "geosite:category-ir"
                },
                IP = new()
                {
                    "geoip:private",
                    "geoip:ir"
                }
            };

            bool hasNoise = noise_out != null || noiseIPv4_out != null || noiseIPv6_out != null;
            ConfigRouting.Rule rule101 = new()
            {
                InboundTag = new()
                {
                    socks_in.Tag
                },
                OutboundTag = fragment_out.Tag,
                Network = hasNoise ? ConfigRouting.Rule.Get.Network.Tcp : ConfigRouting.Rule.Get.Network.TcpUdp
            };

            // Add Routing Rules
            xrayConfig.Routing.Rules.Add(rule1);
            xrayConfig.Routing.Rules.Add(rule2);
            if (addGeo)
            {
                xrayConfig.Routing.Rules.Add(rule3);
                xrayConfig.Routing.Rules.Add(rule4);
            }
            xrayConfig.Routing.Rules.Add(rule101);

            if (hasNoise)
            {
                if (noise_out != null)
                {
                    ConfigRouting.Rule rule102 = new()
                    {
                        InboundTag = new()
                        {
                            socks_in.Tag
                        },
                        OutboundTag = noise_out.Tag,
                        Network = ConfigRouting.Rule.Get.Network.Udp
                    };
                    xrayConfig.Routing.Rules.Add(rule102);
                }
                else
                {
                    if (noiseIPv4_out != null)
                    {
                        ConfigRouting.Rule rule102 = new()
                        {
                            InboundTag = new()
                            {
                                socks_in.Tag
                            },
                            OutboundTag = noiseIPv4_out.Tag,
                            Network = ConfigRouting.Rule.Get.Network.Udp,
                            IP = new()
                            {
                                "0.0.0.0/0"
                            }
                        };
                        xrayConfig.Routing.Rules.Add(rule102);
                    }

                    if (noiseIPv6_out != null)
                    {
                        ConfigRouting.Rule rule102 = new()
                        {
                            InboundTag = new()
                            {
                                socks_in.Tag
                            },
                            OutboundTag = noiseIPv6_out.Tag,
                            Network = ConfigRouting.Rule.Get.Network.Udp,
                            IP = new()
                            {
                                "::/0"
                            }
                        };
                        xrayConfig.Routing.Rules.Add(rule102);
                    }
                }
            }

            xrayConfig.Log.DnsLog = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ConfigBuilder Build_Serverless_Internal: " + ex.Message);
        }

        return xrayConfig;
    }

}