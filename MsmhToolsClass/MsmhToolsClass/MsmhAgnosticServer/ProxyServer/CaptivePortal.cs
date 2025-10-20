namespace MsmhToolsClass.MsmhAgnosticServer;

internal class CaptivePortal
{
    private readonly List<string> CaptivePortals = new();

    public CaptivePortal()
    {
        CaptivePortals = new()
        {
            "clients1.google.com",
            "clients2.google.com",
            "clients3.google.com",
            "clients4.google.com",
            "clients5.google.com",
            "clients6.google.com",
            "captive.apple.com",
            "connectivitycheck.android.com",
            "connectivitycheck.gstatic.com",
            "cp.cloudflare.com",
            "detectportal.firefox.com",
            "ipv6.msftncsi.com",
            "ipv6.msftncsi.com.edgesuite.net",
            "msftconnecttest.com",
            "msftncsi.com",
            "msftncsi.com.edgesuite.net",
            "nmcheck.gnome.org",
            "teredo.ipv6.microsoft.com",
            "teredo.ipv6.microsoft.com.nsatc.net",
            "wifi.connected.xfinity.com",
            "www.msftconnecttest.com",
            "www.msftncsi.com",
            "www.msftncsi.com.edgesuite.net",
        };
    }

    public bool IsCaptivePortal(string address)
    {
        return CaptivePortals.IsContain(address);
    }
}