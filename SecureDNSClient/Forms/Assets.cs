using MsmhToolsClass;
using System.Diagnostics;
using System.Text;

namespace SecureDNSClient;

public partial class FormMain
{
    public async Task<(bool IR_Domains_Done, bool IR_CIDRs_Done, bool IR_ADS_Done)> Assets_Download_Async(int timeoutMs = 8000)
    {
        bool autoUpdate = false;
        int hours = 6;
        
        bool ir_Domains = false, ir_CIDRs = false, ir_ADS = false;
        bool ir_Domains_Done = false, ir_CIDRs_Done = false, ir_ADS_Done = false;
        string ir_Domains_msg = "// IR Domains For SDC - Secure DNS Client";
        string ir_CIDRs_msg = "// IR CIDRs For SDC - Secure DNS Client";
        string ir_ADS_msg = "// IR ADS Domains For SDC - Secure DNS Client";

        try
        {
            this.InvokeIt(() =>
            {
                autoUpdate = CustomCheckBoxGeoAssetUpdate.Checked;
                hours = Convert.ToInt32(CustomNumericUpDownGeoAssetsUpdate.Value);
                ir_Domains = CustomCheckBoxGeoAsset_IR_Domains.Checked;
                ir_CIDRs = CustomCheckBoxGeoAsset_IR_CIDRs.Checked;
                ir_ADS = CustomCheckBoxGeoAsset_IR_ADS.Checked;
            });

            TimeSpan hoursTS = new(hours, 0, 0);
            DateTime now = DateTime.UtcNow;

            try
            {
                if (autoUpdate)
                {
                    FileInfo fi = new(SecureDNS.Asset_IR_Domains);
                    if (fi.Exists && (now - fi.LastWriteTimeUtc) < hoursTS) ir_Domains = false;

                    fi = new(SecureDNS.Asset_IR_CIDRs);
                    if (fi.Exists && (now - fi.LastWriteTimeUtc) < hoursTS) ir_CIDRs = false;

                    fi = new(SecureDNS.Asset_IR_ADS_Domains);
                    if (fi.Exists && (now - fi.LastWriteTimeUtc) < hoursTS) ir_ADS = false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Assets Assets_Download_Async FileInfo: " + e.Message);
            }

            bool go = ir_Domains || ir_CIDRs || ir_ADS;
            if (go)
            {
                List<string> releaseURLs = await WebAPI.Github_Latest_Release_Async("msasanmh", "Iran-clash-rules", timeoutMs);
                foreach (string releaseURL in releaseURLs)
                {
                    // IR_Domains
                    if (ir_Domains && releaseURL.EndsWith("ir.txt"))
                    {
                        List<string> result = await Assets_Get_From_Clash_Async(releaseURL, timeoutMs).ConfigureAwait(false);
                        if (result.Count > 0)
                        {
                            result.Insert(0, ir_Domains_msg);
                            await result.SaveToFileAsync(SecureDNS.Asset_IR_Domains);
                            ir_Domains_Done = true;
                        }
                    }

                    // IR_CIDRs
                    if (ir_CIDRs && releaseURL.EndsWith("ircidr.txt"))
                    {
                        List<string> result = await Assets_Get_From_CIDR_Async(releaseURL, timeoutMs).ConfigureAwait(false);
                        if (result.Count > 0)
                        {
                            result.Insert(0, ir_CIDRs_msg);
                            await result.SaveToFileAsync(SecureDNS.Asset_IR_CIDRs);
                            ir_CIDRs_Done = true;
                        }
                    }

                    // IR_ADS
                    if (ir_ADS && releaseURL.EndsWith("ads.txt"))
                    {
                        List<string> result = await Assets_Get_From_Clash_Async(releaseURL, timeoutMs).ConfigureAwait(false);
                        if (result.Count > 0)
                        {
                            result.Insert(0, ir_ADS_msg);
                            await result.SaveToFileAsync(SecureDNS.Asset_IR_ADS_Domains);
                            ir_ADS_Done = true;
                        }
                    }
                }

                // Mirror: IR_Domains
                if (ir_Domains && !ir_Domains_Done)
                {
                    string mirror = "https://raw.githubusercontent.com/msasanmh/SecureDNSClient/refs/heads/main/Assets/IR_Domains.txt";
                    byte[] bytes = await WebAPI.DownloadFileAsync(mirror, timeoutMs).ConfigureAwait(false);
                    if (bytes.Length > 0)
                    {
                        await File.WriteAllBytesAsync(SecureDNS.Asset_IR_Domains, bytes);
                        ir_Domains_Done = true;
                    }
                }

                // Mirror: IR_CIDRs
                if (ir_CIDRs && !ir_CIDRs_Done)
                {
                    string mirror = "https://raw.githubusercontent.com/msasanmh/SecureDNSClient/refs/heads/main/Assets/IR_CIDRs.txt";
                    byte[] bytes = await WebAPI.DownloadFileAsync(mirror, timeoutMs).ConfigureAwait(false);
                    if (bytes.Length > 0)
                    {
                        await File.WriteAllBytesAsync(SecureDNS.Asset_IR_CIDRs, bytes);
                        ir_CIDRs_Done = true;
                    }
                }

                // Mirror: IR_ADS
                if (ir_ADS &&  !ir_ADS_Done)
                {
                    string mirror = "https://raw.githubusercontent.com/msasanmh/SecureDNSClient/refs/heads/main/Assets/IR_ADS_Domains.txt";
                    byte[] bytes = await WebAPI.DownloadFileAsync(mirror, timeoutMs).ConfigureAwait(false);
                    if (bytes.Length > 0)
                    {
                        await File.WriteAllBytesAsync(SecureDNS.Asset_IR_ADS_Domains, bytes);
                        ir_ADS_Done = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Assets Assets_Download_Async: " + ex.Message);
        }

        return (ir_Domains_Done, ir_CIDRs_Done, ir_ADS_Done);
    }

    public static async Task<List<string>> Assets_Get_From_Clash_Async(string url, int timeoutMs)
    {
        List<string> result = new();

        try
        {
            byte[] bytes = await WebAPI.DownloadFileAsync(url, timeoutMs).ConfigureAwait(false);
            if (bytes.Length > 0)
            {
                string text = Encoding.UTF8.GetString(bytes);
                List<string> lines = text.SplitToLines();

                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n].Trim();
                    if (line.StartsWith('#')) continue;
                    if (line.StartsWith("+."))
                    {
                        line = line.TrimStart("+.");
                        if (line.Contains('.'))
                        {
                            result.Add(line);
                            result.Add($"*.{line}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Assets Assets_Get_From_Clash_Async: " + ex.Message);
        }

        return result;
    }

    public static async Task<List<string>> Assets_Get_From_CIDR_Async(string url, int timeoutMs)
    {
        List<string> result = new();

        try
        {
            byte[] bytes = await WebAPI.DownloadFileAsync(url, timeoutMs).ConfigureAwait(false);
            if (bytes.Length > 0)
            {
                string text = Encoding.UTF8.GetString(bytes);
                List<string> lines = text.SplitToLines();

                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n].Trim();
                    if (line.StartsWith("//")) continue;
                    if (line.StartsWith('#')) continue;
                    if (line.Contains('/'))
                    {
                        result.Add(line);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Assets Assets_Get_From_CIDR_Async: " + ex.Message);
        }

        return result;
    }

}