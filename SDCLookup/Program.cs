using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace SDCLookup;

internal static partial class Program
{
    [STAThread]
    static async Task Main()
    {
        try
        {
            // e.g. (If Contains Space Must Be In Double Quotation ")
            // -Domain=google.com -DNSs=dns1,dns2 -TimeoutMS=5000 -DoubleCheck=True
            // -Domain=google.com -DNSs="dns 1, dns 2" -TimeoutMS=5000 -DoubleCheck=True

            // Title
            string title = $"SDC Lookup v{Assembly.GetExecutingAssembly().GetName().Version}";
            if (OperatingSystem.IsWindows()) Console.Title = title;

            // Invariant Culture
            Info.SetCulture(CultureInfo.InvariantCulture);

            string domain = string.Empty;
            DnsEnums.RRType rrType = DnsEnums.RRType.A;
            DnsEnums.CLASS qClass = DnsEnums.CLASS.IN;
            List<string> dnss = new();
            int timeoutMS = 10000;
            bool insecure = false;
            IPAddress bootstrapIP = IPAddress.None;
            int bootstrapPort = 0;
            string proxyScheme = string.Empty;
            string proxyUser = string.Empty;
            string proxyPass = string.Empty;
            bool doubleCheck = false;

            string[] args = Environment.GetCommandLineArgs();
            for (int n = 0; n < args.Length; n++)
            {
                try
                {
                    string arg = args[n];

                    KeyValue kv = GetValue(arg, Key.Domain, typeof(string));
                    if (kv.IsSuccess) domain = kv.ValueString;

                    kv = GetValue(arg, Key.Type, typeof(string));
                    if (kv.IsSuccess) rrType = DnsEnums.ParseRRType(kv.ValueString);

                    kv = GetValue(arg, Key.Class, typeof(string));
                    if (kv.IsSuccess) qClass = DnsEnums.ParseClass(kv.ValueString);

                    kv = GetValue(arg, Key.DNSs, typeof(string));
                    if (kv.IsSuccess) dnss = kv.ValueString.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

                    kv = GetValue(arg, Key.TimeoutMS, typeof(int));
                    if (kv.IsSuccess) timeoutMS = kv.ValueInt;

                    kv = GetValue(arg, Key.Insecure, typeof(bool));
                    if (kv.IsSuccess) insecure = kv.ValueBool;

                    kv = GetValue(arg, Key.BootstrapIP, typeof(string));
                    if (kv.IsSuccess)
                    {
                        bool isIP = IPAddress.TryParse(kv.ValueString, out IPAddress? bootIP);
                        if (isIP && bootIP != null) bootstrapIP = bootIP;
                    }

                    kv = GetValue(arg, Key.BootstrapPort, typeof(int));
                    if (kv.IsSuccess) bootstrapPort = kv.ValueInt;

                    kv = GetValue(arg, Key.ProxyScheme, typeof(string));
                    if (kv.IsSuccess) proxyScheme = kv.ValueString;

                    kv = GetValue(arg, Key.ProxyUser, typeof(string));
                    if (kv.IsSuccess) proxyUser = kv.ValueString;

                    kv = GetValue(arg, Key.ProxyPass, typeof(string));
                    if (kv.IsSuccess) proxyPass = kv.ValueString;

                    kv = GetValue(arg, Key.DoubleCheck, typeof(bool));
                    if (kv.IsSuccess) doubleCheck = kv.ValueBool;
                }
                catch (Exception) { }
            }

            string result = string.Empty;
            Debug.WriteLine(dnss.ToString(Environment.NewLine));
            if (!string.IsNullOrEmpty(domain) && dnss.Count > 0)
            {
                bool hasLocalIp = false;
                int recordCount = 0;
                int latency = -1;
                DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, domain, rrType, qClass);
                bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
                if (isWriteSuccess)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnss, insecure, bootstrapIP, bootstrapPort, timeoutMS, proxyScheme, proxyUser, proxyPass).ConfigureAwait(false);
                    sw.Stop();

                    if (doubleCheck)
                    {
                        sw.Restart();
                        dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnss, insecure, bootstrapIP, bootstrapPort, timeoutMS, proxyScheme, proxyUser, proxyPass).ConfigureAwait(false);
                        sw.Stop();
                    }
                    
                    latency = Convert.ToInt32(sw.ElapsedMilliseconds);
                    if (dmABuffer.Length >= 12) // 12 Header Length
                    {
                        DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);
                        if (dmA.IsSuccess)
                        {
                            if (dmA.Header.AnswersCount > 0 && dmA.Answers.AnswerRecords.Count > 0)
                            {
                                for (int n = 0; n < dmA.Answers.AnswerRecords.Count; n++)
                                {
                                    IResourceRecord irr = dmA.Answers.AnswerRecords[n];

                                    if (irr is ARecord aRecord)
                                    {
                                        if (NetworkTool.IsLocalIP(aRecord.IP.ToString()) ||
                                            IPAddress.IsLoopback(aRecord.IP)) hasLocalIp = true;
                                        else recordCount++;
                                    }

                                    if (irr is AaaaRecord aaaaRecord)
                                    {
                                        if (IPAddress.IsLoopback(aaaaRecord.IP) || 
                                            aaaaRecord.IP.IsIPv6LinkLocal ||
                                            aaaaRecord.IP.IsIPv6SiteLocal) hasLocalIp = true;
                                        else recordCount++;
                                    }
                                }
                            }

                            bool isOnline = !hasLocalIp && recordCount > 0;

                            result = $"{latency}{Environment.NewLine}";
                            result += $"{isOnline}{Environment.NewLine}";
                            result += dmA.ToString();

                            Console.Out.WriteLine(result);
                        }
                    }
                }
            }
            else
            {
                result = "Wronge Command.";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine(result);
                Console.ResetColor();
            }

            if (string.IsNullOrEmpty(result))
            {
                result = "Error: Server Not Responding.";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine(result);
                Console.ResetColor();
            }
        }
        catch (Exception) { }
    }
}