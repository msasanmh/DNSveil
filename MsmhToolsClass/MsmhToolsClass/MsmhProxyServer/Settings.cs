using System.Net;

#nullable enable
namespace MsmhToolsClass.MsmhProxyServer;

/// <summary>
/// Proxy server settings.
/// </summary>
public class ProxySettings
{
    private IPAddress ListenerIpAddress_ { get; set; } = IPAddress.Any;
    
    /// <summary>
    /// The DNS hostname or IP address on which to listen.
    /// </summary>
    public IPAddress ListenerIpAddress
    {
        get => ListenerIpAddress_;
        set
        {
            ListenerIpAddress_ = value ?? throw new ArgumentNullException(nameof(ListenerIpAddress));
        }
    }

    private int ListenerPort_ { get; set; } = 10800;

    /// <summary>
    /// The TCP port on which to listen.
    /// </summary>
    public int ListenerPort
    {
        get => ListenerPort_;
        set
        {
            if (value < 0 || value > 65535) throw new ArgumentOutOfRangeException(nameof(ListenerPort));
            ListenerPort_ = value;
        }
    }

    private int MaxRequests_ { get; set; } = 600;

    /// <summary>
    /// Maximum number of threads per second. (Min: 20)
    /// </summary>
    public int MaxRequests
    {
        get => MaxRequests_;
        set
        {
            MaxRequests_ = value >= MsmhProxyServer.MaxRequestsDivide ? value : 600;
        }
    }

    /// <summary>
    /// Kill Request if didn't receive data for n seconds. Default: 0 Sec (Disabled)
    /// </summary>
    public int RequestTimeoutSec { get; set; } = 0;

    /// <summary>
    /// Kill Requests If CPU Usage is Higher than this Value. (Windows Only)
    /// </summary>
    public float KillOnCpuUsage { get; set; } = 40;

    public bool BlockPort80 { get; set; } = false;

}