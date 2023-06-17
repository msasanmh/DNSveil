using MsmhTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecureDNSClient.DNSCrypt
{
    public class DNSCryptConfigEditor
    {
        private List<string> ConfigList = new();
        private string ConfigPath = string.Empty;
        public DNSCryptConfigEditor(string configPath)
        {
            ConfigPath = configPath;
            ConfigList.Clear();
            string text = File.ReadAllText(configPath);
            ConfigList = text.SplitToLines();
        }

        public void EditDnsCache(bool enable)
        {
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (line.Contains("cache = true") || line.Contains("cache = false"))
                {
                    // e.g. cache = true
                    if (enable)
                        ConfigList[n] = "cache = true";
                    else
                        ConfigList[n] = "cache = false";
                    break;
                }
            }
        }

        public void EditHTTPProxy(string proxyScheme)
        {
            if (string.IsNullOrEmpty(proxyScheme)) return;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n];
                if (line.Contains("http_proxy"))
                {
                    // e.g. http_proxy = 'https://http.proxy.net:8080'
                    ConfigList[n] = "http_proxy = '" + proxyScheme + "'";
                    break;
                }
            }
        }

        public void RemoveHTTPProxy()
        {
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n];
                if (line.Contains("http_proxy"))
                {
                    // e.g. http_proxy = 'https://http.proxy.net:8080'
                    ConfigList[n] = "#http_proxy = ''";
                    break;
                }
            }
        }

        public void EditBootstrapDNS(IPAddress bootstrapDNS, int bootstrapPort)
        {
            if (bootstrapDNS == null) return;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n];
                if (line.Contains("bootstrap_resolvers"))
                {
                    // e.g. bootstrap_resolvers = ['9.9.9.11:53', '1.1.1.1:53']
                    ConfigList[n] = $"bootstrap_resolvers = ['{bootstrapDNS}:{bootstrapPort}', '1.1.1.1:53']";
                    break;
                }
            }
        }

        public void EditCertPath(string certPath)
        {
            if (string.IsNullOrEmpty(certPath)) return;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n];
                if (line.Contains("cert_file"))
                {
                    // e.g. cert_file = 'certs/domain.crt'
                    ConfigList[n] = "cert_file = '" + certPath + "'";
                    break;
                }
            }
        }

        public void EditCertKeyPath(string certKeyPath)
        {
            if (string.IsNullOrEmpty(certKeyPath)) return;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n];
                if (line.Contains("cert_key_file"))
                {
                    // e.g. cert_key_file = 'certs/domain.key'
                    ConfigList[n] = "cert_key_file = '" + certKeyPath + "'";
                    break;
                }
            }
        }

        public void EnableDoH()
        {
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n];
                if (line.Contains("0.0.0.0:443"))
                {
                    // e.g. listen_addresses = ['0.0.0.0:443']
                    ConfigList[n] = "listen_addresses = ['0.0.0.0:443']";
                    break;
                }
            }
        }

        public void DisableDoH()
        {
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n];
                if (line.Contains("0.0.0.0:443"))
                {
                    // e.g. listen_addresses = ['0.0.0.0:443']
                    ConfigList[n] = "#listen_addresses = ['0.0.0.0:443']";
                    break;
                }
            }
        }

        public async Task WriteAsync()
        {
            if (!FileDirectory.IsFileLocked(ConfigPath))
            {
                File.WriteAllText(ConfigPath, string.Empty);
                for (int n = 0; n < ConfigList.Count; n++)
                {
                    string line = ConfigList[n];
                    
                    if (n == ConfigList.Count - 1)
                        await FileDirectory.AppendTextAsync(ConfigPath, line, new UTF8Encoding(false));
                    else
                        await FileDirectory.AppendTextLineAsync(ConfigPath, line, new UTF8Encoding(false));
                }
                //File.WriteAllLines(ConfigPath, ConfigList);
            }
        }
    }
}
