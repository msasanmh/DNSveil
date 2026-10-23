using DNSveil.Platform;
using DNSveil.Platform.Abstractions;

if (OperatingSystem.IsLinux())
{
    // Late-bound so Windows builds do not require Linux assembly
    var bootstrapType = Type.GetType("DNSveil.Platform.Linux.LinuxBootstrap, DNSveil.Platform.Linux");
    bootstrapType?.GetMethod("Initialize")?.Invoke(null, null);
}

var argsList = args.ToList();
if (argsList.Count == 0 || argsList[0] is "-h" or "--help" or "help")
{
    PrintHelp();
    return 0;
}

var cmd = argsList[0].ToLowerInvariant();
try
{
    return await RunAsync(cmd, argsList.Skip(1).ToArray());
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 1;
}

static async Task<int> RunAsync(string cmd, string[] rest)
{
    switch (cmd)
    {
        case "start":
            return await ServiceAsync(s => s.StartAsync());
        case "stop":
            return await ServiceAsync(s => s.StopAsync());
        case "restart":
            return await ServiceAsync(s => s.RestartAsync());
        case "status":
            return await StatusAsync();
        case "dns":
            return await DnsAsync(rest);
        case "proxy":
            return ProxyStatus(rest);
        case "config":
            return ConfigAsync(rest);
        case "version":
        case "--version":
            Console.WriteLine("dnsveil 0.1.0 (Linux foundation)");
            return 0;
        default:
            Console.Error.WriteLine($"Unknown command: {cmd}");
            PrintHelp();
            return 1;
    }
}

static async Task<int> ServiceAsync(Func<IServiceManager, Task> action)
{
    var svc = PlatformFactory.CreateServiceManager();
    if (!svc.IsSupported)
    {
        Console.Error.WriteLine("Service management is not available on this system.");
        return 1;
    }
    await action(svc);
    var st = await svc.GetStatusAsync();
    Console.WriteLine(st);
    return 0;
}

static async Task<int> StatusAsync()
{
    Console.WriteLine($"Platform : {(PlatformFactory.IsLinux ? "Linux" : PlatformFactory.IsWindows ? "Windows" : "Other")}");

    try
    {
        var net = PlatformFactory.CreateNetworkConfigurator();
        Console.WriteLine($"Config   : {net.ConfigRoot}");
        Console.WriteLine($"Data     : {net.DataRoot}");
        Console.WriteLine($"Log      : {net.LogRoot}");
    }
    catch (PlatformNotSupportedException) { }

    try
    {
        var dns = PlatformFactory.CreateDnsManager();
        Console.WriteLine($"DNS backend: {dns.DetectBackend()}");
        Console.WriteLine($"Privileged : {dns.HasPrivilege()}");
        var servers = await dns.GetCurrentDnsAsync();
        Console.WriteLine($"DNS servers: {(servers.Count == 0 ? "(none)" : string.Join(", ", servers))}");
    }
    catch (PlatformNotSupportedException ex)
    {
        Console.WriteLine($"DNS: {ex.Message}");
    }

    try
    {
        var svc = PlatformFactory.CreateServiceManager();
        if (svc.IsSupported)
        {
            var st = await svc.GetStatusAsync();
            Console.WriteLine($"Service  : {st}");
        }
    }
    catch { }

    return 0;
}

static async Task<int> DnsAsync(string[] rest)
{
    if (rest.Length == 0)
    {
        Console.WriteLine("usage: dnsveil dns list|set <server...>|restore");
        return 1;
    }

    var dns = PlatformFactory.CreateDnsManager();
    switch (rest[0].ToLowerInvariant())
    {
        case "list":
        case "show":
        {
            Console.WriteLine($"Backend: {dns.DetectBackend()}");
            var servers = await dns.GetCurrentDnsAsync();
            foreach (var s in servers) Console.WriteLine(s);
            return 0;
        }
        case "set":
        {
            if (rest.Length < 2)
            {
                Console.Error.WriteLine("usage: dnsveil dns set <server> [server...]");
                return 1;
            }
            await dns.SetDnsAsync(rest.Skip(1).ToArray());
            Console.WriteLine("DNS updated.");
            return 0;
        }
        case "restore":
        {
            await dns.RestoreDnsAsync();
            Console.WriteLine("DNS restored.");
            return 0;
        }
        case "scan":
            Console.WriteLine("DNS scan: use SDCLookup / GUI scanner (not yet ported to CLI).");
            return 0;
        default:
            Console.Error.WriteLine($"Unknown dns subcommand: {rest[0]}");
            return 1;
    }
}

static int ProxyStatus(string[] rest)
{
    Console.WriteLine("Proxy status: managed by SDCAgnosticServer when running.");
    Console.WriteLine("Start the local proxy via configuration or future 'dnsveil proxy start'.");
    return 0;
}

static int ConfigAsync(string[] rest)
{
    try
    {
        var net = PlatformFactory.CreateNetworkConfigurator();
        if (rest.Length == 0 || rest[0] is "show")
        {
            Console.WriteLine($"config: {net.ConfigRoot}");
            Console.WriteLine($"data  : {net.DataRoot}");
            Console.WriteLine($"log   : {net.LogRoot}");
            var cfg = Path.Combine(net.ConfigRoot, "dnsveil.json");
            if (File.Exists(cfg))
                Console.WriteLine(File.ReadAllText(cfg));
            else
                Console.WriteLine("(no dnsveil.json yet)");
            return 0;
        }
        if (rest[0] == "set" && rest.Length >= 3)
        {
            Directory.CreateDirectory(net.ConfigRoot);
            var cfg = Path.Combine(net.ConfigRoot, "dnsveil.json");
            // Minimal key=value store for bootstrap
            var dict = File.Exists(cfg)
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(cfg))
                  ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
            dict[rest[1]] = string.Join(' ', rest.Skip(2));
            File.WriteAllText(cfg, System.Text.Json.JsonSerializer.Serialize(dict, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("ok");
            return 0;
        }
    }
    catch (PlatformNotSupportedException ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }

    Console.WriteLine("usage: dnsveil config [show|set <key> <value>]");
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine(
@"DNSveil CLI - secure DNS client (Linux foundation)

Usage:
  dnsveil <command> [args]

Commands:
  start              Start dnsveil.service (systemd)
  stop               Stop dnsveil.service
  restart            Restart dnsveil.service
  status             Show platform / DNS / service status

  dns list           List current system DNS servers
  dns set <ip...>    Set system DNS (needs root)
  dns restore        Restore previous DNS
  dns scan           (placeholder) DNS scanner

  proxy status       Proxy status

  config show        Show config paths and file
  config set k v     Set a config key

  version            Print version
  help               This help");
}
