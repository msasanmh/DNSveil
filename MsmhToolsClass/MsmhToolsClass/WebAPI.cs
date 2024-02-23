using System.Diagnostics;

namespace MsmhToolsClass;

public class WebAPI
{
    public static async Task<int> CheckMaliciousUrl_IpQualityScore(string url, string apiKey, int timeoutMs = 30000)
    {
        int result = -1;

        try
        {
            using HttpClient httpClient = new();
            httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://ipqualityscore.com/api/json/url?key={apiKey}&url={url}"),
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

                List<string> strings = JsonTool.GetValues(jsonString, new List<string>() { "risk_score" });
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
            Debug.WriteLine("CheckMaliciousUrl_IpQualityScore: " + ex.Message);
        }

        return result;
    }
}
