namespace MsmhToolsClass.MsmhAgnosticServer;

internal class CaptivePortal
{
    private readonly List<string> CaptivePortals = new();

    public CaptivePortal()
    {
        CaptivePortals = new()
        {
            "www.msftconnecttest.com",
            "msftconnecttest.com",
            "www.msftncsi.com",
            "msftncsi.com",
            "www.msftncsi.com.edgesuite.net",
            "msftncsi.com.edgesuite.net",
            "ipv6.msftncsi.com",
            "ipv6.msftncsi.com.edgesuite.net",
            "teredo.ipv6.microsoft.com",
            "teredo.ipv6.microsoft.com.nsatc.net",
            "connectivitycheck.android.com",
            "connectivitycheck.gstatic.com",
            "clients1.google.com",
            "clients2.google.com",
            "clients3.google.com",
            "clients4.google.com",
            "clients5.google.com",
            "clients6.google.com",
            "captive.apple.com",
            "detectportal.firefox.com",
            "nmcheck.gnome.org",
            "wifi.connected.xfinity.com"
        };
    }

    public bool IsCaptivePortal(string address)
    {
        return CaptivePortals.IsContain(address);
    }
}