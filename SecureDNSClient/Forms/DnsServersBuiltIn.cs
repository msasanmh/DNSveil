using MsmhToolsClass;
using System.Diagnostics;
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

    public static async Task<List<ReadDnsResult>> ReadBuiltInServersAsync(CheckRequest checkRequest, bool readInsecure, bool skipDownload = false)
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
                    Uri uri = new(url, UriKind.Absolute);

                    HttpRequest hr = new()
                    {
                        AllowAutoRedirect = true,
                        AllowInsecure = true,
                        TimeoutMS = 20000,
                        URI = uri
                    };

                    HttpRequestResponse hrr = await HttpRequest.SendAsync(hr);
                    if (hrr.IsSuccess)
                    {
                        try
                        {
                            content = Encoding.UTF8.GetString(hrr.Data);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        // Try With System Proxy
                        string systemProxyScheme = NetworkTool.GetSystemProxy();
                        if (!string.IsNullOrWhiteSpace(systemProxyScheme))
                        {
                            hr.ProxyScheme = systemProxyScheme;
                            hrr = await HttpRequest.SendAsync(hr);
                            if (hrr.IsSuccess)
                            {
                                try
                                {
                                    content = Encoding.UTF8.GetString(hrr.Data);
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(content))
                {
                    // Save Download Servers To File
                    try
                    {
                        await File.WriteAllTextAsync(file, content, new UTF8Encoding(false));
                        Debug.WriteLine("====================> Built-In Servers Downloaded And Saved To " + file);
                    }
                    catch (Exception) { }
                }
                else
                {
                    // Try Read From File
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
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        // Save Resource Servers To File
                        try
                        {
                            await File.WriteAllTextAsync(file, content, new UTF8Encoding(false));
                        }
                        catch (Exception) { }
                    }
                }

                if (!string.IsNullOrWhiteSpace(content))
                    output = await ReadBuiltInServersByContentAsync(content, checkRequest);
            }
            catch (Exception) { }

            return output;
        });

        return await rbs.WaitAsync(CancellationToken.None);
    }
}