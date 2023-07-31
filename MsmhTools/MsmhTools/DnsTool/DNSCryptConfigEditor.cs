using MsmhTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.DnsTool
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
            string keyName = "http_proxy";
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (line.Contains(keyName))
                {
                    // e.g. http_proxy = 'https://http.proxy.net:8080'
                    ConfigList[n] = $"{keyName} = '{proxyScheme}'";
                    break;
                }
            }
        }

        public void RemoveHTTPProxy()
        {
            string keyName = "http_proxy";
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (line.Contains(keyName))
                {
                    // e.g. http_proxy = 'https://http.proxy.net:8080'
                    ConfigList[n] = $"#{keyName} = ''";
                    break;
                }
            }
        }

        public void EditBootstrapDNS(IPAddress bootstrapDNS, int bootstrapPort)
        {
            if (bootstrapDNS == null) return;
            string keyName = "bootstrap_resolvers";
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (line.Contains(keyName))
                {
                    // e.g. bootstrap_resolvers = ['9.9.9.11:53', '1.1.1.1:53']
                    ConfigList[n] = $"{keyName} = ['{bootstrapDNS}:{bootstrapPort}', '1.1.1.1:53']";
                    break;
                }
            }
        }

        public void EditCertPath(string certPath)
        {
            if (string.IsNullOrEmpty(certPath)) return;
            string sectionName = "[local_doh]";
            string keyName = "cert_file";
            bool section = false;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (!section && line.StartsWith(sectionName))
                    section = true;

                if (section)
                {
                    if (line.Contains(keyName))
                    {
                        // e.g. cert_file = 'certs/domain.crt'
                        ConfigList[n] = $"{keyName} = '{certPath}'";
                        break;
                    }

                    // Break if reaches next section
                    if (line.StartsWith('[') && !line.StartsWith(sectionName)) break;
                }
            }
        }

        public void EditCertKeyPath(string certKeyPath)
        {
            if (string.IsNullOrEmpty(certKeyPath)) return;
            string sectionName = "[local_doh]";
            string keyName = "cert_key_file";
            bool section = false;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (!section && line.StartsWith(sectionName))
                    section = true;

                if (section)
                {
                    if (line.Contains(keyName))
                    {
                        // e.g. cert_key_file = 'certs/domain.key'
                        ConfigList[n] = $"{keyName} = '{certKeyPath}'";
                        break;
                    }

                    // Break if reaches next section
                    if (line.StartsWith('[') && !line.StartsWith(sectionName)) break;
                }
            }
        }

        public void EnableDoH(int dohPort)
        {
            string sectionName = "[local_doh]";
            string keyName = "listen_addresses";
            bool section = false;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (!section && line.StartsWith(sectionName))
                    section = true;
                
                if (section)
                {
                    if (line.Contains(keyName))
                    {
                        // e.g. listen_addresses = ['0.0.0.0:443']
                        ConfigList[n] = $"{keyName} = ['0.0.0.0:{dohPort}']";
                        break;
                    }

                    // Break if reaches next section
                    if (line.StartsWith('[') && !line.StartsWith(sectionName)) break;
                }
            }
        }

        public void DisableDoH()
        {
            string sectionName = "[local_doh]";
            string keyName = "listen_addresses";
            bool section = false;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (!section && line.StartsWith(sectionName))
                    section = true;

                if (section)
                {
                    if (line.Contains(keyName))
                    {
                        // e.g. listen_addresses = ['0.0.0.0:443']
                        ConfigList[n] = $"#{keyName} = ['0.0.0.0:443']";
                        break;
                    }

                    // Break if reaches next section
                    if (line.StartsWith('[') && !line.StartsWith(sectionName)) break;
                }
            }
        }

        public void ChangePersonalServer(string[] sdns)
        {
            string sectionName = "[static]";
            string keyName = "stamp";
            bool section = false;
            for (int n = 0; n < ConfigList.Count; n++)
            {
                string line = ConfigList[n].Trim();
                if (!section && line.StartsWith(sectionName))
                    section = true;

                if (section)
                {
                    // Remove all existing personal servers
                    if (n < ConfigList.Count - 1)
                    {
                        ConfigList.RemoveRange(n + 1, ConfigList.Count - (n + 1));
                    }
                    
                    // e.g. [static.Personal]
                    // e.g. stamp = 'sdns://AgcAAAAAAAAABzEuMC4wLjEAEmRucy5jbG91ZGZsYXJlLmNvbQovZG5zLXF1ZXJ5'
                    for (int i = 0; i < sdns.Length; i++)
                    {
                        ConfigList.Add(string.Empty);
                        string newLine1 = $"[static.Personal{i + 1}]";
                        ConfigList.Add(newLine1);

                        string sdnsOne = sdns[i];
                        string newLine2 = $"{keyName} = '{sdnsOne}'";
                        ConfigList.Add(newLine2);
                    }
                    
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
