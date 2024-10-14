using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;

namespace SDCAgnosticServer;

public class ServerProfile
{
    public string Name { get; set; } = string.Empty;
    public MsmhAgnosticServer? AgnosticServer { get; set; }
    public AgnosticSettings? Settings { get; set; }
    public AgnosticSettingsSSL? SettingsSSL { get; set; }
    public AgnosticProgram.Fragment? Fragment { get; set; }
    public AgnosticProgram.Rules? Rules { get; set; }
    public AgnosticProgram.DnsLimit? DnsLimit { get; set; }
}

public static partial class Program
{
    public static readonly List<ServerProfile> ServerProfiles = new();

    private static async void AddProfile(ServerProfile serverProfile)
    {
        try
        {
            if (string.IsNullOrEmpty(serverProfile.Name)) return;

            bool isProfileExist = false;
            foreach (ServerProfile existServerProfile in ServerProfiles)
            {
                if (serverProfile.Name.Equals(existServerProfile.Name) && existServerProfile.AgnosticServer != null)
                {
                    isProfileExist = true;
                    if (serverProfile.Settings != null) existServerProfile.Settings = serverProfile.Settings;
                    if (serverProfile.SettingsSSL != null)
                    {
                        existServerProfile.SettingsSSL = serverProfile.SettingsSSL;
                        await existServerProfile.AgnosticServer.EnableSSL(existServerProfile.SettingsSSL);
                    }
                    if (serverProfile.Fragment != null)
                    {
                        existServerProfile.Fragment = serverProfile.Fragment;
                        existServerProfile.AgnosticServer.EnableFragment(existServerProfile.Fragment);
                    }
                    if (serverProfile.Rules != null)
                    {
                        existServerProfile.Rules = serverProfile.Rules;
                        existServerProfile.AgnosticServer.EnableRules(existServerProfile.Rules);
                    }
                    if (serverProfile.DnsLimit != null)
                    {
                        existServerProfile.DnsLimit = serverProfile.DnsLimit;
                        existServerProfile.AgnosticServer.EnableDnsLimit(existServerProfile.DnsLimit);
                    }
                }
            }

            if (!isProfileExist)
            {
                serverProfile.AgnosticServer = new();
                ServerProfiles.Add(serverProfile);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Profiles AddProfile: " + ex.Message);
        }
    }
}