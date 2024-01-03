namespace SDCProxyServer;

public struct Key
{
    public readonly struct ParentProcess
    {
        public static readonly string Name = "ParentProcess";
        public static readonly string PID = "PID";
    }
    public readonly struct Setting
    {
        public static readonly string Name = "Setting";
        public static readonly string Port = "Port";
        public static readonly string MaxRequests = "MaxRequests";
        public static readonly string RequestTimeoutSec = "RequestTimeoutSec";
        public static readonly string KillOnCpuUsage = "KillOnCpuUsage";
        public static readonly string BlockPort80 = "BlockPort80";
    }

    public readonly struct SSLSetting
    {
        public static readonly string Name = "SSLSetting";
        public static readonly string Enable = "Enable";
        public static readonly string RootCA_Path = "RootCA_Path";
        public static readonly string RootCA_KeyPath = "RootCA_KeyPath";
        public static readonly string ChangeSni = "ChangeSni";
        public static readonly string DefaultSni = "DefaultSni";
    }

    public readonly struct Programs
    {
        public static readonly string Name = "Programs";

        public readonly struct BwList
        {
            public static readonly string Name = "BwList";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string BlackListFile = "BlackListFile";
                public static readonly string BlackListText = "BlackListText";
                public static readonly string WhiteListFile = "WhiteListFile";
                public static readonly string WhiteListText = "WhiteListText";
                public static readonly string Disable = "Disable";
            }
            public static readonly string PathOrText = "PathOrText";
        }

        public readonly struct Dns
        {
            public static readonly string Name = "Dns";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string DoH = "DoH";
                public static readonly string PlainDNS = "PlainDNS";
                public static readonly string System = "System";
                public static readonly string Disable = "Disable";
            }
            public static readonly string TimeoutSec = "TimeoutSec";
            public static readonly string DnsAddr = "DnsAddr";
            public static readonly string DnsCleanIp = "DnsCleanIp";
            public static readonly string ProxyScheme = "ProxyScheme";
            public static readonly string CfCleanIp = "CfCleanIp";
            public static readonly string CfIpRange = "CfIpRange";
        }

        public readonly struct DontBypass
        {
            public static readonly string Name = "DontBypass";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string File = "File";
                public static readonly string Text = "Text";
                public static readonly string Disable = "Disable";
            }
            public static readonly string PathOrText = "PathOrText";
        }

        public readonly struct DpiBypass
        {
            public static readonly string Name = "DpiBypass";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public readonly struct Program
                {
                    public static readonly string Name = "Program";
                    public static readonly string BeforeSniChunks = "BeforeSniChunks";
                    public readonly struct ChunkMode
                    {
                        public static readonly string Name = "ChunkMode";
                        public static readonly string SNI = "SNI";
                        public static readonly string SniExtension = "SniExtension";
                        public static readonly string AllExtensions = "AllExtensions";
                    }
                    public static readonly string SniChunks = "SniChunks";
                    public static readonly string AntiPatternOffset = "AntiPatternOffset";
                    public static readonly string FragmentDelay = "FragmentDelay";
                }
                public static readonly string Disable = "Disable";
            }
        }

        public readonly struct FakeDns
        {
            public static readonly string Name = "FakeDns";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string File = "File";
                public static readonly string Text = "Text";
                public static readonly string Disable = "Disable";
            }
            public static readonly string PathOrText = "PathOrText";
        }

        public readonly struct FakeSni
        {
            public static readonly string Name = "FakeSni";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string File = "File";
                public static readonly string Text = "Text";
                public static readonly string Disable = "Disable";
            }
            public static readonly string PathOrText = "PathOrText";
        }

        public readonly struct UpStreamProxy
        {
            public static readonly string Name = "UpStreamProxy";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string HTTP = "HTTP";
                public static readonly string SOCKS5 = "SOCKS5";
                public static readonly string Disable = "Disable";
            }
            public static readonly string Host = "Host";
            public static readonly string Port = "Port";
            public static readonly string OnlyApplyToBlockedIPs = "OnlyApplyToBlockedIPs";
        }
    }
}