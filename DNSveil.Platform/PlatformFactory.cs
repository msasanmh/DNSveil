using DNSveil.Platform.Abstractions;

namespace DNSveil.Platform;

/// <summary>
/// Creates platform-specific implementations. Register concrete factories
/// from Windows / Linux assemblies at startup.
/// </summary>
public static class PlatformFactory
{
    private static Func<ISystemDnsManager>? _dnsFactory;
    private static Func<IServiceManager>? _serviceFactory;
    private static Func<IPacketInterceptor>? _packetFactory;
    private static Func<IDpiBypass>? _dpiFactory;
    private static Func<INetworkConfigurator>? _netFactory;

    public static void Register(
        Func<ISystemDnsManager>? dns = null,
        Func<IServiceManager>? service = null,
        Func<IPacketInterceptor>? packet = null,
        Func<IDpiBypass>? dpi = null,
        Func<INetworkConfigurator>? net = null)
    {
        if (dns != null) _dnsFactory = dns;
        if (service != null) _serviceFactory = service;
        if (packet != null) _packetFactory = packet;
        if (dpi != null) _dpiFactory = dpi;
        if (net != null) _netFactory = net;
    }

    public static ISystemDnsManager CreateDnsManager()
        => _dnsFactory?.Invoke() ?? throw new PlatformNotSupportedException("ISystemDnsManager not registered.");

    public static IServiceManager CreateServiceManager()
        => _serviceFactory?.Invoke() ?? throw new PlatformNotSupportedException("IServiceManager not registered.");

    public static IPacketInterceptor CreatePacketInterceptor()
        => _packetFactory?.Invoke() ?? throw new PlatformNotSupportedException("IPacketInterceptor not registered.");

    public static IDpiBypass CreateDpiBypass()
        => _dpiFactory?.Invoke() ?? throw new PlatformNotSupportedException("IDpiBypass not registered.");

    public static INetworkConfigurator CreateNetworkConfigurator()
        => _netFactory?.Invoke() ?? throw new PlatformNotSupportedException("INetworkConfigurator not registered.");

    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsWindows => OperatingSystem.IsWindows();
}
