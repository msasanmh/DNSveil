using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class Endless
{
    private readonly ConcurrentDictionary<string, bool> Caches = new();

    private readonly AgnosticSettings? Settings;
    private readonly AgnosticSettingsSSL? SettingsSSL;

    public Endless(AgnosticSettings settings, AgnosticSettingsSSL settingsSSL)
    {
        Settings = settings;
        SettingsSSL = settingsSSL;
    }

    public Endless() { }

    public bool IsUpstreamEqualToServerAddress(string? proxyScheme)
    {
        try
        {
            if (Settings != null && SettingsSSL != null && !string.IsNullOrEmpty(proxyScheme))
            {
                bool isCached = Caches.TryGetValue(proxyScheme, out bool isEqualToServer);
                if (isCached)
                {
                    return isEqualToServer;
                }
                else
                {
                    
                    bool value = Internal_IsUpstreamEqualToServerAddress(proxyScheme);
                    Caches.TryAdd(proxyScheme, value);
                    return value;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Endless IsUpstreamEqualToServerAddress: " + ex.Message);
            return false;
        }
    }

    public void Clear()
    {
        try
        {
            Caches.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Endless Clear: " + ex.Message);
        }
    }

    public bool Internal_IsUpstreamEqualToServerAddress(string? proxyScheme)
    {
        bool result = false;

        try
        {
            if (Settings != null && SettingsSSL != null && !string.IsNullOrEmpty(proxyScheme))
            {
                NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(proxyScheme, 443);
                if (Settings.ListenerPort == urid.Port)
                {
                    bool isIP = NetworkTool.IsIP(urid.Host, out IPAddress? ip);
                    if (isIP && ip != null)
                    {
                        if (IPAddress.IsLoopback(ip) || ip.Equals(Settings.LocalIpAddress)) result = true;
                    }
                    else
                    {
                        if (urid.Host.ToLower().Equals("localhost")) result = true;
                        else if (urid.Host.ToLower().Equals(SettingsSSL.ServerDomainName)) result = true;
                    }
                }
            }
        }
        catch (Exception) { }

        return result;
    }

}