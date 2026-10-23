using System.Runtime.Versioning;
using DNSveil.Platform.Abstractions;
using DNSveil.Platform.Linux.Dns;
using DNSveil.Platform.Linux.Dpi;
using DNSveil.Platform.Linux.Network;
using DNSveil.Platform.Linux.Service;

namespace DNSveil.Platform.Linux;

/// <summary>
/// Call once at process start on Linux to register platform services.
/// </summary>
[SupportedOSPlatform("linux")]
public static class LinuxBootstrap
{
    public static void Initialize()
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("LinuxBootstrap is only for Linux.");

        PlatformFactory.Register(
            dns: () => new LinuxSystemDnsManager(),
            service: () => new SystemdServiceManager(),
            packet: () => new LinuxPacketInterceptor(),
            dpi: () => new LinuxDpiBypass(),
            net: () => new LinuxNetworkConfigurator());
    }
}
