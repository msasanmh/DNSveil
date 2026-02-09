using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

namespace SecureDNSClient;

public partial class FormMain
{
    public static async Task<List<string>> BuiltInEncryptAsync(List<string> dnss)
    {
        List<string> encryptedDnss = new();

        try
        {
            byte[] mpub = await ResourceTool.GetResourceBinFileAsync("SecureDNSClient.MPub.bin", Assembly.GetExecutingAssembly());
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                if (string.IsNullOrWhiteSpace(dns)) continue;
                bool isSuccess = CypherRSA.TryEncrypt(dns, mpub, out _, out string encryptedDns);
                if (isSuccess) encryptedDnss.Add(encryptedDns);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("BuiltInEncryptAsync: " + ex.Message);
        }

        return encryptedDnss;
    }

    public static async Task<List<string>> BuiltInDecryptAsync(List<string> encryptedDnss)
    {
        List<string> dnss = new();

        try
        {
            byte[] mpriv = await ResourceTool.GetResourceBinFileAsync("SecureDNSClient.MPriv.bin", Assembly.GetExecutingAssembly());
            for (int n = 0; n < encryptedDnss.Count; n++)
            {
                string encryptedDns = encryptedDnss[n];
                if (string.IsNullOrWhiteSpace(encryptedDns)) continue;
                bool isSuccess = CypherRSA.TryDecrypt(encryptedDns, mpriv, out string decryptedDns);
                if (isSuccess) dnss.Add(decryptedDns);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("BuiltInDecryptAsync: " + ex.Message);
        }

        return dnss;
    }

    public static async Task<List<ReadDnsResult>> ReadBuiltInServersByContentAsync(string content, CheckRequest checkRequest)
    {
        Task<List<ReadDnsResult>> rbs = Task.Run(async () =>
        {
            List<ReadDnsResult> output = new();
            if (string.IsNullOrEmpty(content)) return output;

            try
            {
                string[] encryptedDnss = content.ReplaceLineEndings().Split(NL, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                List<string> dnss = await BuiltInDecryptAsync(encryptedDnss.ToList());
                dnss = dnss.Distinct().ToList();
                for (int n = 0; n < dnss.Count; n++)
                {
                    string dns = dnss[n];
                    if (string.IsNullOrWhiteSpace(dns)) continue;
                    ReadDnsResult rdr = new()
                    {
                        DNS = dns,
                        CheckMode = checkRequest.CheckMode,
                        GroupName = checkRequest.GroupName,
                        Description = "Built-In"
                    };
                    output.Add(rdr);
                }
            }
            catch (Exception) { }

            return output;
        });

        return await rbs.WaitAsync(CancellationToken.None);
    }

    private static async Task<List<ReadDnsResult>> ReadBuiltInServers_Internal_Async(CheckRequest checkRequest, bool readInsecure, bool skipDownload = false)
    {
        Task<List<ReadDnsResult>> rbs = Task.Run(async () =>
        {
            List<ReadDnsResult> output = new();

            try
            {
                // Get Content
                string content = string.Empty;

                string url = readInsecure ? SecureDNS.BuiltInServersInsecureUpdateUrl : SecureDNS.BuiltInServersSecureUpdateUrl;
                string file = readInsecure ? SecureDNS.BuiltInServersInsecurePath : SecureDNS.BuiltInServersSecurePath;
                string resource = readInsecure ? "SecureDNSClient.DNS-Servers-Insecure.txt" : "SecureDNSClient.DNS-Servers.txt";

                if (!skipDownload)
                {
                    // Try URL
                    byte[] bytes = await WebAPI.DownloadFileAsync(url, 20000, CancellationToken.None).ConfigureAwait(false);
                    if (bytes.Length > 0)
                    {
                        try
                        {
                            content = Encoding.UTF8.GetString(bytes);
                        }
                        catch (Exception) { }
                    }
                }

                // Try Read From File
                if (string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        content = await File.ReadAllTextAsync(file, new UTF8Encoding(false));
                    }
                    catch (Exception) { }
                }

                // Try Resource
                if (string.IsNullOrWhiteSpace(content))
                {
                    content = await ResourceTool.GetResourceTextFileAsync(resource, Assembly.GetExecutingAssembly());
                }

                if (!string.IsNullOrWhiteSpace(content))
                    output = await ReadBuiltInServersByContentAsync(content, checkRequest);
            }
            catch (Exception) { }

            return output;
        });

        return await rbs.WaitAsync(CancellationToken.None);
    }

    public static async Task<List<ReadDnsResult>> ReadBuiltInServersAsync(CheckRequest checkRequest, bool readInsecure, bool skipDownload = false)
    {
        List<ReadDnsResult> output = await ReadBuiltInServers_Internal_Async(checkRequest, false, skipDownload);
        if (readInsecure)
        {
            output.AddRange(await ReadBuiltInServers_Internal_Async(checkRequest, true, skipDownload));
        }

        try
        {
            List<string> allDNSs = new();
            List<string> urlsOrFiles = new()
            {
                "https://github.com/DNSCrypt/dnscrypt-resolvers/blob/master/v3/public-resolvers.md",
                "https://github.com/curl/curl/wiki/DNS-over-HTTPS"
            };
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000, CancellationToken.None);
                allDNSs.AddRange(dnss);
            }
            allDNSs = allDNSs.Distinct().ToList();
            allDNSs = await DnsTools.DecodeStampAsync(allDNSs);
            allDNSs = allDNSs.Distinct().ToList();
            for (int n = 0; n < allDNSs.Count; n++)
            {
                string dns = allDNSs[n];
                ReadDnsResult rdr = new()
                {
                    DNS = dns,
                    CheckMode = checkRequest.CheckMode,
                    GroupName = checkRequest.GroupName,
                    Description = "Built-In"
                };
                output.Add(rdr);
            }

            output = output.DistinctBy(x => x.DNS).ToList();
        }
        catch (Exception) { }

        return output;
    }

    public async Task<List<ReadDnsResult>> ReadBuiltInServersAndSubsAsync(CheckRequest checkRequest, bool readInsecure, bool isBackground = false)
    {
        Task<List<ReadDnsResult>> rbs = Task.Run(async () =>
        {
            List<ReadDnsResult> output = new();

            try
            {
                // Get
                List<string> allDNSs = new();

                string url_Secure = SecureDNS.BuiltInServersSecureUpdateUrl;
                string file_Secure = SecureDNS.BuiltInServersSecurePath;
                string resource_Secure = "SecureDNSClient.DNS-Servers.txt";

                string url_Insecure = SecureDNS.BuiltInServersInsecureUpdateUrl;
                string file_Insecure = SecureDNS.BuiltInServersInsecurePath;
                string resource_Insecure = "SecureDNSClient.DNS-Servers-Insecure.txt";

                // Try URL Built-In Secure
                string encryptedContent = string.Empty;
                string msg = string.Empty;

                if (!isBackground)
                {
                    msg = $"Downloading {url_Secure}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                }
                
                byte[] bytes = await WebAPI.DownloadFileAsync(url_Secure, 20000, CancellationToken.None).ConfigureAwait(false);
                if (StopChecking) return output;
                if (bytes.Length > 0)
                {
                    try
                    {
                        encryptedContent = Encoding.UTF8.GetString(bytes);
                        // Save Downloaded Servers To File
                        if (!string.IsNullOrWhiteSpace(encryptedContent))
                        {
                            await File.WriteAllTextAsync(file_Secure, encryptedContent, new UTF8Encoding(false));
                            if (StopChecking) return output;
                        }
                    }
                    catch (Exception) { }
                }
                else
                {
                    if (!isBackground)
                    {
                        msg = $"Download Failed. Reading Backup...{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DarkOrange));
                    }
                    
                    if (File.Exists(file_Secure))
                    {
                        try
                        {
                            // Try Read From File
                            encryptedContent = await File.ReadAllTextAsync(file_Secure, new UTF8Encoding(false));
                            if (StopChecking) return output;
                        }
                        catch (Exception) { }
                    }
                    if (string.IsNullOrEmpty(encryptedContent))
                    {
                        // Try Read From Resource
                        encryptedContent = await ResourceTool.GetResourceTextFileAsync(resource_Secure, Assembly.GetExecutingAssembly());
                        if (StopChecking) return output;
                    }
                }

                string[] encryptedDnss = encryptedContent.ReplaceLineEndings().Split(NL, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                List<string> dnss = await BuiltInDecryptAsync(encryptedDnss.ToList());
                if (StopChecking) return output;
                allDNSs.AddRange(dnss);

                if (!isBackground)
                {
                    msg = $"Fetched {dnss.Count} Servers.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }
                
                if (readInsecure)
                {
                    // Try URL Built-In Insecure
                    if (!isBackground)
                    {
                        msg = $"Downloading {url_Insecure}{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                    }
                    
                    bytes = await WebAPI.DownloadFileAsync(url_Insecure, 20000, CancellationToken.None).ConfigureAwait(false);
                    if (StopChecking) return output;
                    if (bytes.Length > 0)
                    {
                        try
                        {
                            encryptedContent = Encoding.UTF8.GetString(bytes);
                            // Save Downloaded Servers To File
                            if (!string.IsNullOrWhiteSpace(encryptedContent))
                            {
                                await File.WriteAllTextAsync(file_Insecure, encryptedContent, new UTF8Encoding(false));
                                if (StopChecking) return output;
                            }
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        if (!isBackground)
                        {
                            msg = $"Download Failed. Reading Backup...{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DarkOrange));
                        }
                        
                        if (File.Exists(file_Insecure))
                        {
                            try
                            {
                                // Try Read From File
                                encryptedContent = await File.ReadAllTextAsync(file_Insecure, new UTF8Encoding(false));
                                if (StopChecking) return output;
                            }
                            catch (Exception) { }
                        }
                        if (string.IsNullOrEmpty(encryptedContent))
                        {
                            // Try Read From Resource
                            encryptedContent = await ResourceTool.GetResourceTextFileAsync(resource_Insecure, Assembly.GetExecutingAssembly());
                            if (StopChecking) return output;
                        }
                    }

                    encryptedDnss = encryptedContent.ReplaceLineEndings().Split(NL, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    dnss = await BuiltInDecryptAsync(encryptedDnss.ToList());
                    if (StopChecking) return output;
                    allDNSs.AddRange(dnss);
                    if (!isBackground)
                    {
                        msg = $"Fetched {dnss.Count} Servers.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                    }
                }

                // Built-In Subs
                List<string> allSubs = new();
                List<string> urlsOrFiles = new()
                {
                    "https://github.com/DNSCrypt/dnscrypt-resolvers/blob/master/v3/public-resolvers.md",
                    "https://github.com/curl/curl/wiki/DNS-over-HTTPS",
                    "https://adguard-dns.io/kb/general/dns-providers",
                    "https://github.com/NiREvil/vless/blob/main/DNS%20over%20HTTPS/any%20DNS-over-HTTPS%20server%20you%20want.md",
                    "https://gist.githubusercontent.com/mutin-sa/5dcbd35ee436eb629db7872581093bc5/raw/e69d04151312208dc023c124ebdda22832db4325/Top_Public_Recursive_Name_Servers.md",
                };

                if (urlsOrFiles.Count > 0)
                {
                    for (int n = 0; n < urlsOrFiles.Count; n++)
                    {
                        string urlOrFile = urlsOrFiles[n];
                        if (!isBackground)
                        {
                            msg = $"Downloading {WebUtility.UrlDecode(urlOrFile)}{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                        }
                        
                        dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000, CancellationToken.None);
                        if (StopChecking) return output;
                        allSubs.AddRange(dnss);

                        if (!isBackground)
                        {
                            msg = $"Fetched {dnss.Count} Servers.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                        }
                    }

                    allSubs = allSubs.Distinct().ToList();
                    if (allSubs.Count > 0)
                    {
                        // Save Downloaded Servers To File
                        await allSubs.SaveToFileAsync(SecureDNS.BuiltInServersSubscriptionPath);
                        if (StopChecking) return output;
                    }
                    else
                    {
                        if (!isBackground)
                        {
                            msg = $"Download Failed. Reading Backup...{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DarkOrange));
                        }
                        
                        await allSubs.LoadFromFileAsync(SecureDNS.BuiltInServersSubscriptionPath, true, true);
                        if (StopChecking) return output;
                        if (allSubs.Count > 0)
                        {
                            if (!isBackground)
                            {
                                msg = $"Fetched {dnss.Count} Servers.{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                            }
                        }
                        else
                        {
                            if (!isBackground)
                            {
                                msg = $"There Is No Backup.{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DarkOrange));
                            }
                        }
                    }
                }

                if (!isBackground)
                {
                    msg = $"Decoding Stamp Servers...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                }
                
                allDNSs.AddRange(allSubs);
                allDNSs = allDNSs.Distinct().ToList();
                allDNSs = await DnsTools.DecodeStampAsync(allDNSs);
                allDNSs = allDNSs.Distinct().ToList();
                if (StopChecking) return output;

                if (!isBackground)
                {
                    msg = $"Checking For Malicious Servers Based On Your Settings...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                }
                
                string maliciousContent = string.Empty;
                try
                {
                    maliciousContent = await File.ReadAllTextAsync(SecureDNS.BuiltInServersMaliciousPath);
                    if (StopChecking) return output;
                }
                catch (Exception) { }
                List<string> maliciousTemp = maliciousContent.ReplaceLineEndings().Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                List<string> malicious = new();
                for (int n = 0; n < maliciousTemp.Count; n++)
                {
                    string temp = maliciousTemp[n];
                    if (temp.StartsWith("//")) continue;
                    malicious.Add(temp);
                }

                for (int n = 0; n < allDNSs.Count; n++)
                {
                    if (StopChecking) break;
                    string dns = allDNSs[n];

                    // Ignore Malicious Domains
                    DnsReader dr = new(dns);
                    if (malicious.IsContain(dr.Host)) continue;
                    NetworkTool.URL url = NetworkTool.GetUrlOrDomainDetails(dr.Host, dr.Port);
                    if (malicious.IsContain(url.BaseHost)) continue;

                    ReadDnsResult rdr = new()
                    {
                        DNS = dns,
                        CheckMode = checkRequest.CheckMode,
                        GroupName = checkRequest.GroupName,
                        Description = "Built-In"
                    };
                    output.Add(rdr);
                }
            }
            catch (Exception) { }

            return output;
        });

        return await rbs.WaitAsync(CancellationToken.None);
    }

}