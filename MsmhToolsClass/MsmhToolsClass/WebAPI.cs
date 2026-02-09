using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace MsmhToolsClass;

public class WebAPI
{
    private static async Task<byte[]> DownloadFile_Internal_Async(string url, NameValueCollection? headers, int timeoutMS, CancellationToken ct)
    {
        byte[] bytes = Array.Empty<byte>();

        try
        {
            if (url.StartsWith("//", StringComparison.InvariantCulture)) return bytes;
            if (!url.Contains("://", StringComparison.InvariantCulture)) return bytes;
            Uri uri = new(url, UriKind.Absolute);
            HttpRequest hr = new()
            {
                CT = ct,
                AllowAutoRedirect = true,
                Method = HttpMethod.Get,
                AllowInsecure = true,
                TimeoutMS = timeoutMS,
                URI = uri,
                //UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36 Edg/135.0.0.0"
            };

            if (headers != null) hr.Headers = headers;

            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
            if (hrr.IsSuccess)
            {
                bytes = hrr.Data;
            }
            else
            {
                // Try With System Proxy
                string systemProxyScheme = NetworkTool.GetSystemProxy();
                if (!string.IsNullOrWhiteSpace(systemProxyScheme))
                {
                    hr.ProxyScheme = systemProxyScheme;
                    hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
                    if (hrr.IsSuccess) bytes = hrr.Data;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI DownloadFile_Internal_Async: " + ex.Message);
        }

        return bytes;
    }

    public static async Task<byte[]> DownloadFileAsync(string url, int timeoutMS, CancellationToken ct)
    {
        byte[] buffer = await DownloadFile_Internal_Async(url, null, timeoutMS, ct).ConfigureAwait(false);
        if (buffer.Length == 0)
        {
            string githubUserContent = "https://raw.githubusercontent.com";
            if (url.StartsWith(githubUserContent, StringComparison.OrdinalIgnoreCase))
                buffer = await Github_Get_File_AvoidUserContent_Async(url, timeoutMS, ct).ConfigureAwait(false);
        }
        return buffer;
    }

    public static async Task<string> DownloadJsonAsync(string url, int timeoutMS, CancellationToken ct)
    {
        try
        {
            NameValueCollection headers = new()
            {
                { "accept", "application/json" }
            };

            // Github Token
            // { "Authorization", "token YOUR_TOKEN" }

            byte[] buffer = await DownloadFile_Internal_Async(url, headers, timeoutMS, ct).ConfigureAwait(false);
            if (buffer.Length > 0) return Encoding.UTF8.GetString(buffer);
            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI DownloadJsonAsync: " + ex.Message);
            return string.Empty;
        }
    }

    public static async Task<string> GetContentFromTextLinkAsync(string urlOrFile, int timeoutMS, CancellationToken ct)
    {
        try
        {
            byte[] bytes = Array.Empty<byte>();

            if (File.Exists(urlOrFile))
                bytes = await File.ReadAllBytesAsync(urlOrFile, ct);
            else
                bytes = await DownloadFileAsync(urlOrFile, timeoutMS, ct).ConfigureAwait(false);
            
            if (bytes.Length > 0) return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsTools GetContentFromTextLinkAsync: " + ex.Message);
        }

        return string.Empty;
    }

    public static async Task<List<string>> GetLinesFromTextLinkAsync(string urlOrFile, int timeoutMS, CancellationToken ct)
    {
        try
        {
            List<string> allLines = new();
            string content = await GetContentFromTextLinkAsync(urlOrFile, timeoutMS, ct).ConfigureAwait(false);
            if (content.Length > 0)
            {
                List<string> linesResult = content.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                for (int n = 0; n < linesResult.Count; n++)
                {
                    string lineResult = linesResult[n];
                    bool isBase64 = EncodingTool.TryDecodeBase64Url(lineResult, out string result);
                    if (!isBase64) isBase64 = EncodingTool.TryDecodeBase64(lineResult, out result);
                    if (isBase64) lineResult = result;
                    List<string> linesInLineResult = lineResult.SplitToLines(StringSplitOptions.RemoveEmptyEntries);
                    allLines.AddRange(linesInLineResult);
                }
            }
            return allLines.Distinct().ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsTools GetLinesFromTextLinkAsync: " + ex.Message);
            return new List<string>();
        }
    }

    public static async Task<List<string>> ScrapLinksFromTextLinkAsync(string urlOrFile, int timeoutMS, CancellationToken ct)
    {
        try
        {
            string content = await GetContentFromTextLinkAsync(urlOrFile, timeoutMS, ct).ConfigureAwait(false);
            if (content.Length > 0)
            {
                List<string> allLinks = new();
                List<string> lines = await TextTool.RemoveHtmlAndMarkDownTagsAsync(content, true);
                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n].Trim();
                    List<string> links = await TextTool.GetLinksAsync(line);
                    allLinks.AddRange(links);
                }
                return allLinks;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsTools ScrapLinksFromTextLinkAsync: " + ex.Message);
        }

        return new List<string>();
    }

    public static async Task<byte[]> Github_Get_File_Async(string owner, string repo, string pathToFile, int timeoutMS, CancellationToken ct)
    {
        byte[] buffer = Array.Empty<byte>();

        try
        {
            Debug.WriteLine("Using Github API To Get A File Inside A Repo.");
            // Returns JSON With The Base64-Encoded Content Of The File
            pathToFile = pathToFile.TrimStart('/');
            string apiMain = $"https://api.github.com/repos/{owner}/{repo}/contents/{pathToFile}";
            
            string json = await DownloadJsonAsync(apiMain, timeoutMS, ct).ConfigureAwait(false);

            List<JsonTool.JsonPath> path = new()
            {
                new JsonTool.JsonPath("content", 1)
            };

            List<string> base64 = JsonTool.GetValues(json, path);
            string base64Content = base64.ToString(' '); // Ther Is Only One Item
            
            bool isBase64 = EncodingTool.TryDecodeBase64Url(base64Content, out byte[] output);
            if (isBase64) buffer = output;
            else
            {
                isBase64 = EncodingTool.TryDecodeBase64(base64Content, out output);
                if (isBase64) buffer = output;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI Github_Get_File_Async: " + ex.Message);
        }

        return buffer;
    }

    public static async Task<byte[]> Github_Get_File_AvoidUserContent_Async(string githubUserContentURL, int timeoutMS, CancellationToken ct)
    {
        byte[] buffer = Array.Empty<byte>();

        try
        {
            githubUserContentURL = githubUserContentURL.Trim();
            // e.g. https://raw.githubusercontent.com/msasanmh/DNSveil/refs/heads/main/example.txt
            string remove1 = "https://raw.githubusercontent.com/";
            githubUserContentURL = githubUserContentURL.Replace(remove1, string.Empty, StringComparison.OrdinalIgnoreCase);
            string remove2 = "/refs/heads/main";
            githubUserContentURL = githubUserContentURL.Replace(remove2, string.Empty, StringComparison.OrdinalIgnoreCase);
            string remove3 = "/refs/heads";
            githubUserContentURL = githubUserContentURL.Replace(remove3, string.Empty, StringComparison.OrdinalIgnoreCase);
            string remove4 = "/master";
            githubUserContentURL = githubUserContentURL.Replace(remove4, string.Empty);
            // e.g. msasanmh/DNSveil/example.txt

            // Get Owner
            string owner = string.Empty;
            int index = githubUserContentURL.IndexOf('/');
            if (index != -1)
            {
                owner = githubUserContentURL[..index];
                //Debug.WriteLine(owner);
                githubUserContentURL = githubUserContentURL[(index + 1)..];
            }
            // e.g. DNSveil/example.txt

            // Get Repo
            string repo = string.Empty;
            index = githubUserContentURL.IndexOf('/');
            if (index != -1)
            {
                repo = githubUserContentURL[..index];
                //Debug.WriteLine(repo);
                githubUserContentURL = githubUserContentURL[(index + 1)..];
            }

            // Get PathToFile
            string pathToFile = githubUserContentURL;
            //Debug.WriteLine(pathToFile);

            buffer = await Github_Get_File_Async(owner, repo, pathToFile, timeoutMS, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI Github_Get_File_AvoidUserContent_Async: " + ex.Message);
        }

        return buffer;
    }

    /// <summary>
    /// Github Latest Release
    /// </summary>
    /// <returns>Returns Download Links</returns>
    public static async Task<List<string>> Github_Latest_Release_Async(string owner, string repo, int timeoutMS, CancellationToken ct)
    {
        List<string> relaeseURLs = new();

        try
        {
            string apiMain = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            string json = await DownloadJsonAsync(apiMain, timeoutMS, ct).ConfigureAwait(false);

            List<JsonTool.JsonPath> path = new()
            {
                new JsonTool.JsonPath("assets", 1),
                new JsonTool.JsonPath("browser_download_url")
            };

            relaeseURLs = JsonTool.GetValues(json, path);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI Github_Latest_Release_Async: " + ex.Message);
        }
        
        return relaeseURLs;
    }

    /// <summary>
    /// Github Latest PreRelease
    /// </summary>
    /// <returns>Returns Download Links</returns>
    public static async Task<List<string>> Github_Latest_PreRelease_Async(string owner, string repo, int timeoutMS, CancellationToken ct)
    {
        List<string> relaeseURLs = new();

        try
        {
            string apiMain = $"https://api.github.com/repos/{owner}/{repo}/releases";
            string json = await DownloadJsonAsync(apiMain, timeoutMS, ct).ConfigureAwait(false);

            List<JsonTool.JsonPath> path = new()
            {
                new JsonTool.JsonPath("assets", 1) { Conditions = new() { new("prerelease", "true") } },
                new JsonTool.JsonPath("browser_download_url") { Conditions = new() }
            };

            relaeseURLs = JsonTool.GetValues(json, path);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI Github_Latest_PreRelease_Async: " + ex.Message);
        }
        
        return relaeseURLs;
    }

    /// <summary>
    /// Download Speed Test By Cloudflare.
    /// </summary>
    /// <param name="bytes">Minimum Download Payload Size: 5000000 B (5 MB)</param>
    /// <returns>Speed: Bytes Per Seconds.</returns>
    public static async Task<double> Cloudflare_TestDownloadSpeed_Async(string? proxyURL, string? proxyUser, string? proxyPass, int bytes, CancellationToken ct)
    {
        double speed_Bytes_Per_Seconds = 0;
        
        try
        {
            // Recommended Minimum Download Payload Size: 5 MB == 5000000 B
            if (!string.IsNullOrEmpty(proxyURL))
                proxyURL = proxyURL.Replace("socks://", "socks5://", StringComparison.OrdinalIgnoreCase);

            int totalBytes = 0;
            double totalSeconds = 0;
            int payloadSize = 100000;
            double timeoutSec = 30;
            int failedCount = 0;
            int sentbytes = 0;
            while (true)
            {
                Debug.WriteLine($"DL Payload Size: {payloadSize}");
                Debug.WriteLine("DL Timeout Sec: " + timeoutSec);

                string url = $"https://speed.cloudflare.com/__down?bytes={payloadSize}";
                Uri uri = new(url, UriKind.Absolute);

                HttpRequest hr = new()
                {
                    AllowAutoRedirect = true,
                    Method = HttpMethod.Get,
                    AllowInsecure = true,
                    TimeoutMS = TimeSpan.FromSeconds(timeoutSec).TotalMilliseconds.ToInt(),
                    URI = uri,
                    ProxyScheme = proxyURL,
                    ProxyUser = proxyUser,
                    ProxyPass = proxyPass,
                    CT = ct
                };

                Stopwatch sw = Stopwatch.StartNew();
                HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
                sw.Stop();
                Debug.WriteLine("HRR: " + hrr.StatusDescription);
                if (hrr.IsSuccess)
                {
                    totalBytes += hrr.Data.Length;
                    totalSeconds += sw.Elapsed.TotalSeconds;

                    payloadSize += 100000;
                    if (payloadSize > bytes) payloadSize = bytes;
                    timeoutSec = (payloadSize * (sw.Elapsed.TotalSeconds + 5)) / hrr.Data.Length;
                }
                else
                {
                    failedCount++;
                    Debug.WriteLine("Failed: " + failedCount);
                    if (failedCount >= 2) break;
                }

                sentbytes += payloadSize;
                Debug.WriteLine($"DL Bytes Received: {sentbytes} Of {bytes}");
                if (sentbytes >= bytes) break;
            }

            Debug.WriteLine($"DL Payload Size: {payloadSize}");
            Debug.WriteLine("DL Timeout Sec: " + timeoutSec);

            if (totalBytes > 0 && totalSeconds > 0)
            {
                speed_Bytes_Per_Seconds = totalBytes / totalSeconds;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI Cloudflare_TestDownloadSpeed_Async: " + ex.Message);
        }

        return speed_Bytes_Per_Seconds;
    }

    /// <summary>
    /// Upload Speed Test By Cloudflare.
    /// </summary>
    /// <param name="bytes">Minimum Upload Payload Size: 2000000 B (2 MB)</param>
    /// <returns>Speed: Bytes Per Seconds.</returns>
    public static async Task<double> Cloudflare_TestUploadSpeed_Async(string? proxyURL, string? proxyUser, string? proxyPass, int bytes, CancellationToken ct)
    {
        double speed_Bytes_Per_Seconds = 0;
        
        try
        {
            // Recommended Minimum Upload Payload Size: 2 MB == 2000000 B
            if (!string.IsNullOrEmpty(proxyURL))
                proxyURL = proxyURL.Replace("socks://", "socks5://", StringComparison.OrdinalIgnoreCase);

            string url = "https://speed.cloudflare.com/__up";
            Uri uri = new(url, UriKind.Absolute);

            int totalBytes = 0;
            double totalSeconds = 0;
            int payloadSize = 50000;
            double timeoutSec = 30;
            int failedCount = 0;
            int sentbytes = 0;
            while (true)
            {
                Debug.WriteLine($"UL Payload Size: {payloadSize}");
                Debug.WriteLine("UL Timeout Sec: " + timeoutSec);

                byte[] data = new byte[payloadSize];
                Random.Shared.NextBytes(data);

                HttpRequest hr = new()
                {
                    AllowAutoRedirect = true,
                    Method = HttpMethod.Post,
                    DataToSend = data,
                    ContentType = "application/octet-stream",
                    AllowInsecure = true,
                    TimeoutMS = TimeSpan.FromSeconds(timeoutSec).TotalMilliseconds.ToInt(),
                    URI = uri,
                    ProxyScheme = proxyURL,
                    ProxyUser = proxyUser,
                    ProxyPass = proxyPass,
                    CT = ct
                };

                Stopwatch sw = Stopwatch.StartNew();
                HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
                sw.Stop();
                Debug.WriteLine("HRR: " + hrr.StatusDescription);
                if (hrr.IsSuccess)
                {
                    totalBytes += payloadSize;
                    totalSeconds += sw.Elapsed.TotalSeconds;

                    payloadSize += 50000;
                    if (payloadSize > bytes) payloadSize = bytes;
                    timeoutSec = (payloadSize * (sw.Elapsed.TotalSeconds + 5)) / data.Length;
                }
                else
                {
                    failedCount++;
                    Debug.WriteLine("Failed: " + failedCount);
                    if (failedCount >= 2) break;
                }

                sentbytes += payloadSize;
                Debug.WriteLine($"UL Bytes Sent: {sentbytes} Of {bytes}");
                if (sentbytes >= bytes) break;
            }

            Debug.WriteLine($"UL Payload Size: {payloadSize}");
            Debug.WriteLine("UL Timeout Sec: " + timeoutSec);

            if (totalBytes > 0 && totalSeconds > 0)
            {
                speed_Bytes_Per_Seconds = totalBytes / totalSeconds;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI Cloudflare_TestUploadSpeed_Async: " + ex.GetInnerExceptions());
        }

        return speed_Bytes_Per_Seconds;
    }

    public static async Task<List<string>> Cloudflare_CDN_CIDRs_Async(int timeoutMS, CancellationToken ct)
    {
        List<string> result = new();

        try
        {
            string apiMain = "https://api.cloudflare.com/client/v4/ips";
            string json = await DownloadJsonAsync(apiMain, timeoutMS, ct).ConfigureAwait(false);

            List<JsonTool.JsonPath> pathIPv4 = new()
            {
                new JsonTool.JsonPath("result", 1),
                new JsonTool.JsonPath("ipv4_cidrs")
            };

            List<JsonTool.JsonPath> pathIPv6 = new()
            {
                new JsonTool.JsonPath("result", 1),
                new JsonTool.JsonPath("ipv6_cidrs")
            };

            result.AddRange(JsonTool.GetValues(json, pathIPv4));
            result.AddRange(JsonTool.GetValues(json, pathIPv6));
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI Cloudflare_CDN_CIDRs_Async: " + ex.Message);
        }

        return result;
    }

    public static async Task<RegionInfo> GetRegionInfoAsync(int timeoutMS, string? proxyScheme, string? proxyUser, string? proxyPass, CancellationToken ct)
    {
        RegionInfo regionInfo = CultureTool.GetDefaultRegion;

        try
        {
            List<string> apis = new()
            {
                "https://api.ip.sb/geoip",
                "https://free.freeipapi.com/api/json",
                "https://api.country.is",
                "http://ipwho.is/",
                "https://ipapi.co/country" // Plain Text
            };

            bool isSuccess = false;
            Random random = new();
            for (int i = 0; i < apis.Count; i++)
            {
                int n = random.Next(0, apis.Count);
                string api = apis[n];
                HttpRequestResponse hrr = await NetworkTool.GetHttpRequestResponseAsync(api, null, timeoutMS, true, false, false, proxyScheme, proxyUser, proxyPass, ct);
                if (hrr.IsSuccess)
                {
                    string content = Encoding.UTF8.GetString(hrr.Data).Trim();
                    //Debug.WriteLine(content);
                    bool isJson = JsonTool.IsValid(content);
                    if (isJson)
                    {
                        // JSON
                        List<List<JsonTool.JsonPath>> pathsList = new()
                        {
                            new List<JsonTool.JsonPath>() { new("countryCode", 1) },
                            new List<JsonTool.JsonPath>() { new("country_code", 1) },
                            new List<JsonTool.JsonPath>() { new("country-code", 1) },
                            new List<JsonTool.JsonPath>() { new("country", 1) },
                        };

                        for (int j = 0; j < pathsList.Count; j++)
                        {
                            List<JsonTool.JsonPath> paths = pathsList[j];
                            List<string> results = JsonTool.GetValues(content, paths);
                            for (int k = 0; k < results.Count; k++)
                            {
                                string result = results[k].Trim();
                                //Debug.WriteLine(result);
                                RegionInfo ri = CultureTool.GetRegion_ByTwoLetter(result);
                                if (ri.TwoLetterISORegionName.Equals(regionInfo.TwoLetterISORegionName, StringComparison.OrdinalIgnoreCase)) continue;
                                regionInfo = ri;
                                isSuccess = true;

                                //Debug.WriteLine("API: " + api);
                                //Debug.WriteLine(regionInfo.EnglishName);

                                break;
                            }
                            if (isSuccess) break;
                        }
                        if (isSuccess) break;
                    }
                    else
                    {
                        // Plain TEXT
                        if (content.Length == 2)
                        {
                            RegionInfo ri = CultureTool.GetRegion_ByTwoLetter(content);
                            if (ri.TwoLetterISORegionName.Equals(regionInfo.TwoLetterISORegionName, StringComparison.OrdinalIgnoreCase)) continue;
                            regionInfo = ri;
                            isSuccess = true;

                            //Debug.WriteLine("API: " + api);
                            //Debug.WriteLine(regionInfo.EnglishName);

                            break;
                        }
                    }
                    if (isSuccess) break;
                }
                else
                {
                    //Debug.WriteLine("API: " + api);
                    //Debug.WriteLine(hrr.StatusDescription);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI GetRegionInfoAsync: " + ex.Message);
        }

        return regionInfo;
    }

    public static async Task<int> CheckMaliciousUrl_IpQualityScore_Async(string url, string apiKey, int timeoutMS)
    {
        int result = -1;

        try
        {
            using HttpClient httpClient = new();
            httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMS);
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://ipqualityscore.com/api/json/url?key={apiKey}&url={url}", UriKind.Absolute),
                Headers =
                {
                    { "accept", "application/json" }
                }
            };
            using HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
            Debug.WriteLine("CheckMaliciousUrl_IpQualityScore: " + response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(jsonString);

                List<JsonTool.JsonPath> path = new()
                {
                    new JsonTool.JsonPath("risk_score")
                };

                List<string> strings = JsonTool.GetValues(jsonString, path);
                if (strings.Any())
                {
                    string riskScoreStr = strings[0].Trim();
                    int riskScore = Convert.ToInt32(riskScoreStr);
                    Debug.WriteLine("CheckMaliciousUrl_IpQualityScore: " + url + " ==> " + riskScoreStr);
                    result = riskScore; // >= 75 Is Malicious
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI CheckMaliciousUrl_IpQualityScore: " + ex.Message);
        }

        return result;
    }
}
