using System.Diagnostics;
using System.Text;

namespace MsmhToolsClass;

public class WebAPI
{
    public static async Task<byte[]> DownloadFileAsync(string url, int timeoutMs = 5000)
    {
        byte[] bytes = Array.Empty<byte>();

        try
        {
            Uri uri = new(url, UriKind.Absolute);
            HttpRequest hr = new()
            {
                AllowAutoRedirect = true,
                Method = HttpMethod.Get,
                AllowInsecure = true,
                TimeoutMS = timeoutMs,
                URI = uri
            };
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
                    hrr = await HttpRequest.SendAsync(hr);
                    if (hrr.IsSuccess)
                    {
                        bytes = hrr.Data;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebAPI DownloadFileAsync: " + ex.Message);
        }

        return bytes;
    }

    /// <summary>
    /// Github Latest Release
    /// </summary>
    /// <returns>Returns Download Links</returns>
    public static async Task<List<string>> Github_Latest_Release_Async(string owner, string repo, int timeoutMs = 5000)
    {
        List<string> relaeseURLs = new();

        try
        {
            Uri apiMain = new($"https://api.github.com/repos/{owner}/{repo}/releases/latest", UriKind.Absolute);
            HttpRequest hr = new()
            {
                Method = HttpMethod.Get,
                AllowInsecure = true,
                TimeoutMS = timeoutMs,
                URI = apiMain,
                Headers =
                {
                    { "accept", "application/json" }
                }
            };
            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
            string json = Encoding.UTF8.GetString(hrr.Data);

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
    public static async Task<List<string>> Github_Latest_PreRelease_Async(string owner, string repo, int timeoutMs = 5000)
    {
        List<string> relaeseURLs = new();

        try
        {
            Uri apiMain = new($"https://api.github.com/repos/{owner}/{repo}/releases", UriKind.Absolute);
            HttpRequest hr = new()
            {
                Method = HttpMethod.Get,
                AllowInsecure = true,
                TimeoutMS = timeoutMs,
                URI = apiMain,
                Headers =
                {
                    { "accept", "application/json" }
                }
            };
            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
            string json = Encoding.UTF8.GetString(hrr.Data);

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

    public static async Task<List<string>> Cloudflare_CDN_CIDRs_Async(int timeoutMs = 30000)
    {
        List<string> result = new();

        try
        {
            Uri apiMain = new("https://api.cloudflare.com/client/v4/ips", UriKind.Absolute);
            HttpRequest hr = new()
            {
                Method = HttpMethod.Get,
                AllowInsecure = true,
                TimeoutMS = timeoutMs,
                URI = apiMain,
                Headers =
                {
                    { "accept", "application/json" }
                }
            };
            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
            string json = Encoding.UTF8.GetString(hrr.Data);

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

    public static async Task<int> CheckMaliciousUrl_IpQualityScore_Async(string url, string apiKey, int timeoutMs = 30000)
    {
        int result = -1;

        try
        {
            using HttpClient httpClient = new();
            httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://ipqualityscore.com/api/json/url?key={apiKey}&url={url}", UriKind.Absolute),
                Headers =
                {
                    { "accept", "application/json" }
                }
            };
            using HttpResponseMessage response = await httpClient.SendAsync(request);
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
