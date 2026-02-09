using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using MsmhToolsClass.V2RayConfigTool.Inbounds;
using MsmhToolsClass.V2RayConfigTool.Outbounds;
using static MsmhToolsClass.V2RayConfigTool.XrayConfig;

namespace MsmhToolsClass.V2RayConfigTool;

public partial class ConfigBuilder
{
    public static XrayConfig Build(string url)
    {
        return Build_Internal(url, 8053, 8080, null, IPAddress.Parse("8.8.8.8"), 53, false, null);
    }

    public static XrayConfig Build(string url, int listeningDnsPort, int listeningSocksPort)
    {
        return Build_Internal(url, listeningDnsPort, listeningSocksPort, null, IPAddress.Parse("8.8.8.8"), 53, false, null);
    }

    public static XrayConfig Build(string url, int listeningDnsPort, int listeningSocksPort, string? fragmentStr)
    {
        return Build_Internal(url, listeningDnsPort, listeningSocksPort, fragmentStr, IPAddress.Parse("8.8.8.8"), 53, false, null);
    }

    public static XrayConfig Build(string url, int listeningDnsPort, int listeningSocksPort, string? fragmentStr, IPAddress bootstrapIP, int bootstrapPort)
    {
        return Build_Internal(url, listeningDnsPort, listeningSocksPort, fragmentStr, bootstrapIP, bootstrapPort, false, null);
    }

    public static XrayConfig Build(string url, int listeningDnsPort, int listeningSocksPort, string? fragmentStr, IPAddress bootstrapIP, int bootstrapPort, bool addLoopbackDns)
    {
        return Build_Internal(url, listeningDnsPort, listeningSocksPort, fragmentStr, bootstrapIP, bootstrapPort, addLoopbackDns, null);
    }

    public static XrayConfig Build(string url, int listeningDnsPort, int listeningSocksPort, string? fragmentStr, IPAddress bootstrapIP, int bootstrapPort, bool addLoopbackDns, string doh)
    {
        return Build_Internal(url, listeningDnsPort, listeningSocksPort, fragmentStr, bootstrapIP, bootstrapPort, addLoopbackDns, doh);
    }

    private static void RebuildDeserializedXrayConfig(ref XrayConfig? xrayConfig)
    {
        try
        {
            if (xrayConfig != null)
            {
                JsonSerializerOptions jsonSerializerOptions = new()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNameCaseInsensitive = true
                };

                for (int i = 0; i < xrayConfig.Inbounds.Count; i++)
                {
                    ConfigInbound inbound = xrayConfig.Inbounds[i];
                    if (inbound.Settings is JsonElement inboundSettingsElement)
                    {
                        if (inbound.Protocol.Equals(ConfigInbound.Get.Protocol.Dokodemo_door, StringComparison.OrdinalIgnoreCase))
                        {
                            DokoDemoDoorSettingsIn? settingsIn = inboundSettingsElement.Deserialize<DokoDemoDoorSettingsIn>(jsonSerializerOptions);
                            if (settingsIn != null) inbound.Settings = settingsIn;
                        }
                        else if (inbound.Protocol.Equals(ConfigInbound.Get.Protocol.Http, StringComparison.OrdinalIgnoreCase))
                        {
                            HttpSettingsIn? settingsIn = inboundSettingsElement.Deserialize<HttpSettingsIn>(jsonSerializerOptions);
                            if (settingsIn != null) inbound.Settings = settingsIn;
                        }
                        else if (inbound.Protocol.Equals(ConfigInbound.Get.Protocol.Mixed, StringComparison.OrdinalIgnoreCase))
                        {
                            MixedSettingsIn? settingsIn = inboundSettingsElement.Deserialize<MixedSettingsIn>(jsonSerializerOptions);
                            if (settingsIn != null) inbound.Settings = settingsIn;
                        }
                        else if (inbound.Protocol.Equals(ConfigInbound.Get.Protocol.Socks, StringComparison.OrdinalIgnoreCase))
                        {
                            SocksSettingsIn? settingsIn = inboundSettingsElement.Deserialize<SocksSettingsIn>(jsonSerializerOptions);
                            if (settingsIn != null) inbound.Settings = settingsIn;
                        }
                    }
                }

                for (int j = 0; j < xrayConfig.Outbounds.Count; j++)
                {
                    ConfigOutbound outbound = xrayConfig.Outbounds[j];
                    if (outbound.Settings is JsonElement outboundSettingsElement)
                    {
                        Protocol protocol = GetProtocolByConfigOutbound(outbound.Protocol);
                        if (protocol == Protocol.Vless)
                        {
                            VlessSettings? settings = outboundSettingsElement.Deserialize<VlessSettings>(jsonSerializerOptions);
                            if (settings != null) outbound.Settings = settings;
                        }
                        else if (protocol == Protocol.Vmess)
                        {
                            VmessSettings? settings = outboundSettingsElement.Deserialize<VmessSettings>(jsonSerializerOptions);
                            if (settings != null) outbound.Settings = settings;
                        }
                        else if (protocol == Protocol.ShadowSocks)
                        {
                            ShadowsocksSettings? settings = outboundSettingsElement.Deserialize<ShadowsocksSettings>(jsonSerializerOptions);
                            if (settings != null) outbound.Settings = settings;
                        }
                        else if (protocol == Protocol.Trojan)
                        {
                            TrojanSettings? settings = outboundSettingsElement.Deserialize<TrojanSettings>(jsonSerializerOptions);
                            if (settings != null) outbound.Settings = settings;
                        }
                        else if (protocol == Protocol.WireGuard)
                        {
                            WireguardSettings? settings = outboundSettingsElement.Deserialize<WireguardSettings>(jsonSerializerOptions);
                            if (settings != null) outbound.Settings = settings;
                        }
                        else if (protocol == Protocol.HTTP)
                        {
                            HttpSettings? settings = outboundSettingsElement.Deserialize<HttpSettings>(jsonSerializerOptions);
                            if (settings != null) outbound.Settings = settings;
                        }
                        else if (protocol == Protocol.SOCKS)
                        {
                            SocksSettings? settings = outboundSettingsElement.Deserialize<SocksSettings>(jsonSerializerOptions);
                            if (settings != null) outbound.Settings = settings;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ConfigBuilder RebuildDeserializedXrayConfig: " + ex.Message);
        }
    }

    public static XrayConfig? BuildFromJson(string json)
    {
        XrayConfig? xrayConfig = JsonTool.Deserialize<XrayConfig>(json);
        RebuildDeserializedXrayConfig(ref xrayConfig);
        return xrayConfig;
    }

    public static async Task<XrayConfig?> BuildFromJsonAsync(string json)
    {
        XrayConfig? xrayConfig = await JsonTool.DeserializeAsync<XrayConfig>(json);
        RebuildDeserializedXrayConfig(ref xrayConfig);
        return xrayConfig;
    }

    public static string BuildJson(XrayConfig xrayConfig)
    {
        return JsonTool.Serialize(xrayConfig, true);
    }

    public static async Task<string> BuildJsonAsync(XrayConfig xrayConfig)
    {
        return await JsonTool.SerializeAsync(xrayConfig, true);
    }

    private static string GetRemarks(ref string url)
    {
        string remarks = string.Empty;

        try
        {
            char find = '#';
            int index = url.LastIndexOf(find);
            if (index != -1)
            {
                remarks = WebUtility.UrlDecode(url[(index + 1)..]);
                remarks = remarks.ReplaceLineEndings(" ");
                url = url[..index];
            }
        }
        catch (Exception) { }

        return remarks;
    }

    private static bool TryParseVmess(ref string url)
    {
        bool isVmessSuccess = false;

        try
        {
            // Strip Scheme
            string find = "://";
            int index = url.IndexOf(find);
            if (index != -1)
            {
                string vmessBase64 = url[(index + find.Length)..];
                bool isSuccess = EncodingTool.TryDecodeBase64Url(vmessBase64, out string vmessJson);
                if (!isSuccess) isSuccess = EncodingTool.TryDecodeBase64(vmessBase64, out vmessJson);

                if (isSuccess && JsonTool.IsValid(vmessJson))
                {
                    //Debug.WriteLine(vmessJson);
                    VmessUrlModel? vum = JsonTool.Deserialize<VmessUrlModel>(vmessJson);
                    if (vum != null)
                    {
                        if (vum.Address != null)
                        {
                            string vmessUrl = "vmess://";
                            if (vum.ID != null) vmessUrl += $"{vum.ID}@";
                            vmessUrl += vum.Address;
                            if (vum.Port != null) vmessUrl += $":{vum.Port}";
                            vmessUrl += "?protocol=vmess";

                            // Add URL Fragments
                            vmessUrl += $"&aid={vum.AID}&alpn={vum.Alpn}&fp={vum.Fingerprint}&host={vum.Host}&type={vum.Network}&path={vum.Path}";
                            vmessUrl += $"&vmessSecurity={vum.VmessSecurity}&insecure={vum.SkipCertVerify}&sni={vum.Sni}&security={vum.Security}&headerType={vum.Type}&v={vum.V}";

                            // Add Remarks
                            if (vum.Remarks != null) vmessUrl += $"#{vum.Remarks}";
                            
                            url = vmessUrl;
                            isVmessSuccess = true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ConfigBuilder TryParseVmess: {url}{Environment.NewLine}{ex.Message}");
        }

        return isVmessSuccess;
    }

    private static void Parse_HTTP_SOCKS(ref NetworkTool.URL urid)
    {
        try
        {
            string userPassBase64 = urid.Username;
            bool isBase64Url = EncodingTool.TryDecodeBase64Url(userPassBase64, out string userPass);
            if (!isBase64Url) isBase64Url = EncodingTool.TryDecodeBase64(userPassBase64, out userPass);
            if (isBase64Url && userPass.Contains(':'))
            {
                string[] split = userPass.Split(':');
                if (split.Length == 2)
                {
                    urid.Username = split[0];
                    urid.Password = split[1];
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ConfigBuilder Parse_HTTP_SOCKS: " + ex.Message);
        }
    }

    public class GetConfigInfo
    {
        public enum ConfigKind
        {
            Unknown = 0,
            URL = 1,
            JSON = 2,
            XrayModel = 3
        }

        public string UrlOrJson { get; set; } = string.Empty;
        public ConfigKind Kind { get; set; } = ConfigKind.Unknown;
        public Protocol Protocol { get; set; } = Protocol.Unknown;
        public string Remarks { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public Security Security { get; set; } = Security.None;
        public Transport Transport { get; set; } = Transport.TCP;
        /// <summary>
        /// Compare To Find Duplicates
        /// </summary>
        public string IDUnique { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;

        public override string ToString()
        {
            try
            {
                string nl = Environment.NewLine;
                string result = $"{nameof(IsSuccess)}: {IsSuccess}{nl}";
                result += $"{nameof(Kind)}: {Kind}{nl}";
                result += $"{nameof(Protocol)}: {Protocol}{nl}";
                result += $"{nameof(Remarks)}: {Remarks}{nl}";
                result += $"{nameof(Address)}: {Address}{nl}";
                result += $"{nameof(Port)}: {Port}{nl}";
                result += $"{nameof(Security)}: {Security}{nl}";
                result += $"{nameof(Transport)}: {Transport}{nl}";
                result += $"{nameof(IDUnique)}: {IDUnique}";
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConfigBuilder GetConfigInfo ToString: " + ex.Message);
                return string.Empty;
            }
        }

        public GetConfigInfo() { }

        private void SetConfigInfo(XrayConfig config)
        {
            try
            {
                if (!config.IsSuccess) return;

                // Unique ID
                List<string> idUniqueList = new();
                void addToUnique(string? str)
                {
                    if (string.IsNullOrWhiteSpace(str)) return;
                    idUniqueList.Add(str);
                }

                // Set Kind
                if (Kind == ConfigKind.Unknown) Kind = ConfigKind.XrayModel;

                // Set Protocol
                ConfigOutbound mainOutbound = new();
                foreach (ConfigOutbound outbound in config.Outbounds)
                {
                    Protocol protocol = GetProtocolByConfigOutbound(outbound.Protocol);
                    if (protocol != Protocol.Unknown)
                    {
                        Protocol = protocol;
                        mainOutbound = outbound;
                        break;
                    }
                }
                if (Protocol == Protocol.Unknown) return;

                addToUnique(mainOutbound.Protocol);

                // Set Remarks
                Remarks = config.Remarks;

                // Set Address/Port
                if (mainOutbound.Settings is VlessSettings vlessSettings)
                {
                    foreach (var vnext in vlessSettings.Vnext)
                    {
                        if (!string.IsNullOrEmpty(vnext.Address))
                        {
                            Address = vnext.Address;
                            Port = vnext.Port;
                            addToUnique($"{Address}_{Port}");
                            foreach (var user in vnext.Users)
                            {
                                addToUnique(user.ID);
                                addToUnique(user.Encryption);
                                addToUnique(user.Flow);
                            }
                            break;
                        }
                    }
                }
                else if (mainOutbound.Settings is VmessSettings vmessSettings)
                {
                    foreach (var vnext in vmessSettings.Vnext)
                    {
                        if (!string.IsNullOrEmpty(vnext.Address))
                        {
                            Address = vnext.Address;
                            Port = vnext.Port;
                            addToUnique($"{Address}_{Port}");
                            foreach (var user in vnext.Users)
                            {
                                addToUnique(user.ID);
                                addToUnique(user.Security);
                            }
                            break;
                        }
                    }
                }
                else if (mainOutbound.Settings is ShadowsocksSettings shadowsocksSettings)
                {
                    foreach (var server in shadowsocksSettings.Servers)
                    {
                        if (!string.IsNullOrEmpty(server.Address))
                        {
                            Address = server.Address;
                            Port = server.Port;
                            addToUnique($"{Address}_{Port}");
                            addToUnique(server.Email);
                            addToUnique(server.Method);
                            addToUnique(server.Password);
                            break;
                        }
                    }
                }
                else if (mainOutbound.Settings is TrojanSettings trojanSettings)
                {
                    foreach (var server in trojanSettings.Servers)
                    {
                        if (!string.IsNullOrEmpty(server.Address))
                        {
                            Address = server.Address;
                            Port = server.Port;
                            addToUnique($"{Address}_{Port}");
                            addToUnique(server.Email);
                            addToUnique(server.Password);
                            break;
                        }
                    }
                }
                else if (mainOutbound.Settings is WireguardSettings wireguardSettings)
                {
                    addToUnique(wireguardSettings.SecretKey);
                    foreach (string address in wireguardSettings.Address) addToUnique(address);
                    foreach (var peer in wireguardSettings.Peers)
                    {
                        if (!string.IsNullOrEmpty(peer.Endpoint))
                        {
                            string[] split = peer.Endpoint.Split(':');
                            if (split.Length == 2)
                            {
                                Address = split[0];
                                string portStr = split[1];
                                bool isInt = int.TryParse(portStr, out int port);
                                if (isInt)
                                {
                                    Port = port;
                                    addToUnique($"{Address}_{Port}");
                                    break;
                                }
                            }
                        }
                        addToUnique(peer.PreSharedKey);
                        addToUnique(peer.PublicKey);
                    }
                    addToUnique(wireguardSettings.DomainStrategy);
                }
                else if (mainOutbound.Settings is HttpSettings httpSettings)
                {
                    foreach (var server in httpSettings.Servers)
                    {
                        if (!string.IsNullOrEmpty(server.Address))
                        {
                            Address = server.Address;
                            Port = server.Port;
                            addToUnique($"{Address}_{Port}");
                            addToUnique(server.Email);
                            addToUnique(server.Username);
                            addToUnique(server.Password);
                            break;
                        }
                    }
                }
                else if (mainOutbound.Settings is SocksSettings socksSettings)
                {
                    foreach (var server in socksSettings.Servers)
                    {
                        if (!string.IsNullOrEmpty(server.Address))
                        {
                            Address = server.Address;
                            Port = server.Port;
                            addToUnique($"{Address}_{Port}");
                            addToUnique(server.Email);
                            addToUnique(server.Username);
                            addToUnique(server.Password);
                            break;
                        }
                    }
                }
                if (!NetworkTool.IsDomainOrIP(Address)) return;

                // Set Security
                Security = GetSecurity(mainOutbound.StreamSettings.Security);
                addToUnique(mainOutbound.StreamSettings.Security);
                if (mainOutbound.StreamSettings.TlsSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.TlsSettings.ServerName);
                    addToUnique(mainOutbound.StreamSettings.TlsSettings.AllowInsecure.ToString());
                    addToUnique(mainOutbound.StreamSettings.TlsSettings.Alpn.ToString('_'));
                    addToUnique(mainOutbound.StreamSettings.TlsSettings.Fingerprint);
                    addToUnique(mainOutbound.StreamSettings.TlsSettings.CurvePreferences.ToString('_'));
                }
                else if (mainOutbound.StreamSettings.RealitySettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.Target);
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.ServerNames.ToString('_'));
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.PrivateKey);
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.ShortIds.ToString('_'));
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.Fingerprint);
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.ServerName);
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.AllowInsecure.ToString());
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.Alpn.ToString('_'));
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.PublicKey);
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.ShortId);
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.SpiderX);
                    addToUnique(mainOutbound.StreamSettings.RealitySettings.Mldsa65Verify);
                }

                // Set Transport (NetworkType)
                Transport = GetTransport(mainOutbound.StreamSettings.Network);
                addToUnique(mainOutbound.StreamSettings.Network);
                if (mainOutbound.StreamSettings.TcpSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.TcpSettings.Header.Type);
                    addToUnique(mainOutbound.StreamSettings.TcpSettings.Header.Request?.Method);
                    addToUnique(mainOutbound.StreamSettings.TcpSettings.Header.Request?.Path.ToString('_'));
                    addToUnique(mainOutbound.StreamSettings.TcpSettings.Header.Request?.Headers.Host.ToString('_'));
                    addToUnique(mainOutbound.StreamSettings.TcpSettings.Header.Request?.Headers.UserAgent.ToString('_'));
                }
                if (mainOutbound.StreamSettings.HttpSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.HttpSettings.Host.ToString('_'));
                    addToUnique(mainOutbound.StreamSettings.HttpSettings.Path);
                }
                if (mainOutbound.StreamSettings.XHttpSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.XHttpSettings.Host);
                    addToUnique(mainOutbound.StreamSettings.XHttpSettings.Path);
                    addToUnique(mainOutbound.StreamSettings.XHttpSettings.Mode);
                    addToUnique(mainOutbound.StreamSettings.XHttpSettings.Extra?.RootElement.GetRawText());
                }
                if (mainOutbound.StreamSettings.QuicSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.QuicSettings.Security);
                    addToUnique(mainOutbound.StreamSettings.QuicSettings.Key);
                    addToUnique(mainOutbound.StreamSettings.QuicSettings.Header.Type);
                }
                if (mainOutbound.StreamSettings.KcpSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.KcpSettings.Header.Type);
                    addToUnique(mainOutbound.StreamSettings.KcpSettings.Header.Domain);
                    addToUnique(mainOutbound.StreamSettings.KcpSettings.Seed);
                }
                if (mainOutbound.StreamSettings.GrpcSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.GrpcSettings.Authority);
                    addToUnique(mainOutbound.StreamSettings.GrpcSettings.ServiceName);
                    addToUnique(mainOutbound.StreamSettings.GrpcSettings.User_agent);
                }
                if (mainOutbound.StreamSettings.WsSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.WsSettings.Path);
                    addToUnique(mainOutbound.StreamSettings.WsSettings.Host);
                }
                if (mainOutbound.StreamSettings.HttpUpgradeSettings != null)
                {
                    addToUnique(mainOutbound.StreamSettings.HttpUpgradeSettings.Path);
                    addToUnique(mainOutbound.StreamSettings.HttpUpgradeSettings.Host);
                }

                // Add The Rest To Unique List
                if (mainOutbound.StreamSettings.Sockopt != null)
                {
                    addToUnique(mainOutbound.StreamSettings.Sockopt.DomainStrategy);
                    addToUnique(mainOutbound.StreamSettings.Sockopt.DialerProxy);
                }
                addToUnique(mainOutbound.ProxySettings?.Tag);
                addToUnique(mainOutbound.Mux.Enabled.ToString());

                // Set Unique ID
                bool isEncodeSuccess = EncodingTool.TryGetSHA512(idUniqueList.ToString('_'), out string uniqueID);
                if (isEncodeSuccess)
                {
                    IDUnique = uniqueID;
                    IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConfigBuilder GetConfigInfo XrayConfig: " + ex.Message);
            }
        }

        public GetConfigInfo(XrayConfig config)
        {
            SetConfigInfo(config);
        }

        public GetConfigInfo(string urlOrJson)
        {
            try
            {
                urlOrJson = urlOrJson.Trim();
                urlOrJson = urlOrJson.TrimEnd('\u0060'); // `
                urlOrJson = urlOrJson.TrimMiddle('@');
                UrlOrJson = urlOrJson;

                // Decode If It's A Base64Url
                UrlOrJson = UrlOrJson.Trim();
                bool isBase64Url = EncodingTool.TryDecodeBase64Url(UrlOrJson, out string decodedUrl);
                if (!isBase64Url) isBase64Url = EncodingTool.TryDecodeBase64(UrlOrJson, out decodedUrl);
                if (isBase64Url)
                {
                    UrlOrJson = decodedUrl;
                }

                // Get Protocol By URL
                Protocol protocol = GetProtocolByUrl(UrlOrJson);
                if (protocol == Protocol.Unknown)
                {
                    // Check JSON
                    string json = UrlOrJson;
                    bool isJsonValid = JsonTool.IsValid(json);
                    if (isJsonValid)
                    {
                        // Set Kind
                        Kind = ConfigKind.JSON;

                        // Create Model
                        XrayConfig? config = BuildFromJson(json);
                        if (config != null)
                        {
                            SetConfigInfo(config);
                        }
                    }
                }
                else
                {
                    // Set Kind
                    Kind = ConfigKind.URL;

                    XrayConfig config = Build(UrlOrJson);
                    SetConfigInfo(config);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConfigBuilder GetConfigInfo UrlOrJson: " + ex.Message);
            }
        }
    }

    private static XrayConfig Build_Internal(string url, int listeningDnsPort, int listeningSocksPort, string? fragmentStr, IPAddress bootstrapIP, int bootstrapPort, bool addLoopbackDns, string? doh)
    {
        XrayConfig xrayConfig = new();

        try
        {
            // Bools
            bool addFragmentForDnsModule = true;
            bool addGeo = false; // Add geosite/geoip? Not All Clients Are Compatible When There's No .dat File.

            // Decode URL If It's A Base64Url
            url = url.Trim();
            url = url.TrimEnd('\u0060'); // `
            url = url.TrimMiddle('@');
            bool isBase64Url = EncodingTool.TryDecodeBase64Url(url, out string decodedUrl);
            if (!isBase64Url) isBase64Url = EncodingTool.TryDecodeBase64(url, out decodedUrl);
            if (isBase64Url)
            {
                url = decodedUrl;
                url = url.TrimEnd('\u0060'); // `
                url = url.TrimMiddle('@');
            }

            // Get Protocol By URL
            Protocol protocol = GetProtocolByUrl(url);
            if (protocol == Protocol.Unknown)
            {
                Debug.WriteLine("ConfigBuilder Build_Internal - Unknown Protocol: " + url);
                return xrayConfig;
            }

            // Parse Vmess
            if (protocol == Protocol.Vmess)
            {
                bool isVmessSuccess = TryParseVmess(ref url);
                if (!isVmessSuccess) return xrayConfig;
            }

            // Get Remarks
            string remarks = GetRemarks(ref url);

            // Decode scheme://Base64#Remarks
            string find_Scheme = "://";
            int index_Scheme = url.IndexOf(find_Scheme);
            if (index_Scheme != -1)
            {
                string scheme = url[..(index_Scheme + find_Scheme.Length)];
                string tempUrl = url.TrimStart(scheme);
                isBase64Url = EncodingTool.TryDecodeBase64Url(tempUrl, out decodedUrl);
                if (!isBase64Url) isBase64Url = EncodingTool.TryDecodeBase64(tempUrl, out decodedUrl);
                if (isBase64Url)
                {
                    url = $"{scheme}{decodedUrl.Trim()}";
                }
            }

            // Parse URL
            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(url, 443);
            if (urid.Uri == null)
            {
                Debug.WriteLine("ConfigBuilder Build_Internal - URI Is NULL: " + url);
                return xrayConfig;
            }

            // Parse HTTP And SOCKS Protocols
            if (protocol == Protocol.HTTP || protocol == Protocol.SOCKS)
            {
                Parse_HTTP_SOCKS(ref urid);
            }

            //Debug.WriteLine(url);
            // Get URL Fragments
            Dictionary<string, string> kv = NetworkTool.ParseUriQuery(urid.Query, true).ToDictionary();

            // Create Policies
            ConfigPolicy configPolicy = AddPolicies();

            // Create Built-In Dns
            ConfigDns configDns = AddDnsModule(bootstrapIP, bootstrapPort, addLoopbackDns, doh);

            // Create Dns Inbound
            ConfigInbound dns_in = AddInbound_Dns(listeningDnsPort, bootstrapIP, bootstrapPort);

            // Create Socks Inbound
            ConfigInbound socks_in = AddInbound_Socks(listeningSocksPort);

            // Create Direct Outbound
            ConfigOutbound direct_out = AddOutbound_Direct();

            // Create Block Outbound
            ConfigOutbound block_out = AddOutbound_Block();

            // Create DNS Outbound
            ConfigOutbound dns_out = AddOutbound_Dns();

            // Create Fragment Outbound
            string? fragment = !string.IsNullOrEmpty(fragmentStr) ? fragmentStr : kv.GetValueOrDefault("fragment");
            ConfigOutbound? fragment_out = AddOutbound_Freedom("fragment-out", fragment, noise: null);

            // Create Fragment Outbound For DnsModule
            string fragmentForDnsModule = string.Empty;
            if (addFragmentForDnsModule && fragment_out == null)
            {
                fragmentForDnsModule = "tlshello,2-4,3-5";
            }
            ConfigOutbound? fragment_dns_out = AddOutbound_Freedom("fragment-dns-out", fragmentForDnsModule, noise: null);

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
                                    Level = 0
                                }
                            }
                        }
                    }
                };
            }
            else if (protocol == Protocol.Vmess)
            {
                proxy_out.Protocol = ConfigOutbound.Get.Protocol.Vmess;
                proxy_out.Settings = new VmessSettings()
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
                                    Security = kv.GetValueOrDefault("vmesssecurity", VmessSettings.VmessVnext.User.Get.Security.Auto),
                                    Level = 0
                                }
                            }
                        }
                    }
                };
            }
            else if (protocol == Protocol.ShadowSocks)
            {
                string shadowSocksMethod = string.Empty, shadowSocksPassword = string.Empty;
                string methodPassBase64 = urid.Username;
                bool isSuccess = EncodingTool.TryDecodeBase64Url(methodPassBase64, out string methodPass);
                if (!isSuccess) isSuccess = EncodingTool.TryDecodeBase64(methodPassBase64, out methodPass);
                if (isSuccess)
                {
                    if (methodPass.Contains(':'))
                    {
                        string[] split = methodPass.Split(':');
                        if (split.Length == 2)
                        {
                            shadowSocksMethod = split[0];
                            shadowSocksPassword = split[1];
                        }
                    }
                }

                // It's Not Always Encoded In Base64.
                if (!string.IsNullOrEmpty(urid.Password))
                {
                    if (string.IsNullOrEmpty(shadowSocksMethod) || string.IsNullOrEmpty(shadowSocksPassword))
                    {
                        shadowSocksMethod = urid.Username;
                        shadowSocksPassword = urid.Password;
                    }
                }
                
                proxy_out.Protocol = ConfigOutbound.Get.Protocol.Shadowsocks;
                proxy_out.Settings = new ShadowsocksSettings()
                {
                    Servers = new()
                    {
                        new()
                        {
                            Email = kv.GetValueOrDefault("email"),
                            Address = urid.Host,
                            Port = urid.Port,
                            Method = shadowSocksMethod,
                            Password = shadowSocksPassword,
                            Level = 0
                        }
                    }
                };
            }
            else if (protocol == Protocol.Trojan)
            {
                proxy_out.Protocol = ConfigOutbound.Get.Protocol.Trojan;
                proxy_out.Settings = new TrojanSettings()
                {
                    Servers = new()
                    {
                        new()
                        {
                            Email = kv.GetValueOrDefault("email"),
                            Address = urid.Host,
                            Port = urid.Port,
                            Password = urid.Username,
                            Level = 0
                        }
                    }
                };
            }
            else if (protocol == Protocol.WireGuard)
            {
                proxy_out.Protocol = ConfigOutbound.Get.Protocol.Wireguard;
                proxy_out.Settings = new WireguardSettings()
                {
                    SecretKey = urid.Username,
                    Address = kv.GetValueOrDefault("address", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                    Peers = new()
                    {
                        new()
                        {
                            Endpoint = $"{urid.Host}:{urid.Port}",
                            PreSharedKey = kv.GetValueOrDefault("presharedkey"),
                            PublicKey = kv.GetValueOrDefault("publickey"),
                            KeepAlive = 0,
                        }
                    },
                    NoKernelTun = true,
                    MTU = kv.GetValueOrDefault("mtu", "1420").Trim().ToInt(1420),
                    Reserved = kv.GetValueOrDefault("reserved", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToIntList(),
                    Workers = 2,
                    DomainStrategy = WireguardSettings.Get.DomainStrategy.ForceIP
                };
            }
            else if (protocol == Protocol.HTTP)
            {
                proxy_out.Protocol = ConfigOutbound.Get.Protocol.Http;
                proxy_out.Settings = new Outbounds.HttpSettings()
                {
                    Servers = new()
                    {
                        new()
                        {
                            Email = kv.GetValueOrDefault("email"),
                            Address = urid.Host,
                            Port = urid.Port,
                            Username = urid.Username,
                            Password = urid.Password,
                            Level = 0
                        }
                    }
                };
            }
            else if (protocol == Protocol.SOCKS)
            {
                proxy_out.Protocol = ConfigOutbound.Get.Protocol.Socks;
                proxy_out.Settings = new Outbounds.SocksSettings()
                {
                    Servers = new()
                    {
                        new()
                        {
                            Email = kv.GetValueOrDefault("email"),
                            Address = urid.Host,
                            Port = urid.Port,
                            Username = urid.Username,
                            Password = urid.Password,
                            Level = 0
                        }
                    }
                };
            }
            else
            {
                Debug.WriteLine("ConfigBuilder Build_Internal - Unsupported Protocol: " + url);
                return xrayConfig;
            }

            // Set Remarks
            if (!string.IsNullOrEmpty(remarks)) xrayConfig.Remarks = remarks;

            // StreamSettings
            // StreamSettings: Network
            string netwotkType = kv.GetValueOrDefault("type", kv.GetValueOrDefault("network", ConfigOutbound.OutboundStreamSettings.Get.Network.Tcp));
            if (netwotkType.Equals("http", StringComparison.OrdinalIgnoreCase)) netwotkType = "h2";
            proxy_out.StreamSettings.Network = netwotkType;
            
            // StreamSettings: Security
            proxy_out.StreamSettings.Security = kv.GetValueOrDefault("security", ConfigOutbound.OutboundStreamSettings.Get.Security.None);

            // Modify Based On Transport Layer (Network) And Security
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
            else if (fragment_dns_out != null)
            {
                xrayConfig.Outbounds.Add(fragment_dns_out);
                dns_out.StreamSettings.Sockopt.DialerProxy = fragment_dns_out.Tag;
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
                    configDns.Tag
                },
                OutboundTag = fragment_out != null ? fragment_out.Tag : fragment_dns_out != null ? fragment_dns_out.Tag : direct_out.Tag,
                Network = ConfigRouting.Rule.Get.Network.TcpUdp
            };

            ConfigRouting.Rule rule11 = new()
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

            ConfigRouting.Rule rule12 = new()
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
                DomainMatcher = ConfigRouting.Get.DomainMatcher.Hybrid,
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
                xrayConfig.Routing.Rules.Add(rule11);
                xrayConfig.Routing.Rules.Add(rule12);
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
            xrayConfig.IsSuccess = true;
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
            // Bools
            bool addGeo = false; // Add geosite/geoip? Not All Clients Are Compatible When There's No .dat File.

            // Parse URL
            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(url, 443);
            Dictionary<string, string> kv = NetworkTool.ParseUriQuery(urid.Query, true).ToDictionary();

            // Create Policies
            ConfigPolicy configPolicy = AddPolicies();

            // Create Built-In Dns
            ConfigDns configDns = AddDnsModule(IPAddress.Parse("8.8.8.8"), 53, false, null);

            // Create Dns Inbound
            ConfigInbound dns_in = AddInbound_Dns(10853, IPAddress.Parse("8.8.8.8"), 53);

            // Create Socks Inbound
            ConfigInbound socks_in = AddInbound_Socks(10808);

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
            xrayConfig.Remarks = "Serverless Xray Config By DNSveil";

            // StreamSettings: TlsSettings: Fingerprint
            string fingerprint = kv.GetValueOrDefault("fp", kv.GetValueOrDefault("fingerprint", ConfigOutbound.OutboundStreamSettings.StreamTlsSettings.Get.Fingerprint.Chrome));
            
            // Add
            xrayConfig.Policy = configPolicy;
            xrayConfig.Dns = configDns;
            xrayConfig.Inbounds.Add(dns_in);
            xrayConfig.Inbounds.Add(socks_in);
            xrayConfig.Outbounds.Add(direct_out); // Must Be The First Outbound (V2rayNG)
            xrayConfig.Outbounds.Add(block_out);

            fragment_out.StreamSettings.TlsSettings ??= new();
            fragment_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
            xrayConfig.Outbounds.Add(fragment_out);
            dns_out.StreamSettings.Sockopt.DialerProxy = fragment_out.Tag;

            if (noise_out != null)
            {
                noise_out.StreamSettings.TlsSettings ??= new();
                noise_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
                xrayConfig.Outbounds.Add(noise_out);
            }

            if (noiseIPv4_out != null)
            {
                noiseIPv4_out.StreamSettings.TlsSettings ??= new();
                noiseIPv4_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
                xrayConfig.Outbounds.Add(noiseIPv4_out);
            }

            if (noiseIPv6_out != null)
            {
                noiseIPv6_out.StreamSettings.TlsSettings ??= new();
                noiseIPv6_out.StreamSettings.TlsSettings.Fingerprint = fingerprint;
                xrayConfig.Outbounds.Add(noiseIPv6_out);
            }

            xrayConfig.Outbounds.Add(dns_out);

            bool addFakeProxyForOlderCores = true;
            if (addFakeProxyForOlderCores)
            {
                ConfigOutbound fakeOutbound = AddOutbound_FakeVlessForOlderCores();
                fakeOutbound.StreamSettings.Sockopt.DialerProxy = fragment_out.Tag;
                xrayConfig.Outbounds.Add(fakeOutbound);
            }

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
                    configDns.Tag
                },
                OutboundTag = fragment_out.Tag,
                Network = ConfigRouting.Rule.Get.Network.TcpUdp
            };

            ConfigRouting.Rule rule11 = new()
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

            ConfigRouting.Rule rule12 = new()
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
                xrayConfig.Routing.Rules.Add(rule11);
                xrayConfig.Routing.Rules.Add(rule12);
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
            xrayConfig.IsSuccess = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ConfigBuilder Build_Serverless_Internal: " + ex.Message);
        }

        return xrayConfig;
    }

}