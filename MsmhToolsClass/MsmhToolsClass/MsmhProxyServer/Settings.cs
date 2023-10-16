using System;
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

    private int MaxThreads_ { get; set; } = 256;

    /// <summary>
    /// Maximum number of threads to support.
    /// </summary>
    public int MaxThreads
    {
        get => MaxThreads_;
        set
        {
            if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxThreads));
            MaxThreads_ = value;
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