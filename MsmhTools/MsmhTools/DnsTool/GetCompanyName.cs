using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.DnsTool
{
    public static class GetCompanyName
    {
        public static string HostToCompanyOffline(string host, string fileContent)
        {
            host = host.Trim();
            string company = "Couldn't retrieve information.";
            if (!string.IsNullOrWhiteSpace(host))
            {
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    List<string> split = fileContent.SplitToLines();
                    for (int n = 0; n < split.Count; n++)
                    {
                        string hostToCom = split[n];
                        if (hostToCom.Contains(host))
                        {
                            string com = hostToCom.Split('|')[1];
                            if (!string.IsNullOrWhiteSpace(com))
                            {
                                company = com;
                                break;
                            }
                        }
                    }
                }
            }
            return company.Trim();
        }

        public static string UrlToCompanyOffline(string url, string fileContent)
        {
            Network.GetUrlDetails(url, 53, out string host, out int _, out string _, out bool _);
            return HostToCompanyOffline(host, fileContent);
        }

        public static string StampToCompanyOffline(string stampUrl, string fileContent)
        {
            stampUrl = stampUrl.Trim();
            string company = "Couldn't retrieve information.";
            // Can't always return Address
            try
            {
                // Decode Stamp
                DNSCryptStampReader stamp = new(stampUrl);

                if (!string.IsNullOrEmpty(stamp.Host))
                    company = HostToCompanyOffline(stamp.Host, fileContent);
                else if (!string.IsNullOrEmpty(stamp.IP))
                    company = HostToCompanyOffline(stamp.IP, fileContent);
                else
                    company = HostToCompanyOffline(stampUrl, fileContent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return company;
        }
    }
}
