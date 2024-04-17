namespace SDCAgnosticServer;

public struct Key
{
    public readonly struct Common
    {
        public static readonly string C = "C";
        public static readonly string CLS = "CLS";
        public static readonly string Load = "Load";
        public static readonly string Profile = "Profile";
        public static readonly string Out = "Out";
        public static readonly string Status = "Status";
        public static readonly string Flush = "Flush";
        public static readonly string Start = "Start";
        public static readonly string Stop = "Stop";
        public static readonly string Save = "Save";
        public static readonly string KillAll = "KillAll";
        public static readonly string Requests = "Requests";
        public static readonly string FragmentDetails = "FragmentDetails";
    }

    public readonly struct ParentProcess
    {
        public static readonly string Name = "ParentProcess";
        public static readonly string PID = "PID";
    }

    public readonly struct Setting
    {
        public static readonly string Name = "Setting";
        public static readonly string Port = "Port";
        public static readonly string WorkingMode = "WorkingMode";
        public static readonly string MaxRequests = "MaxRequests";
        public static readonly string DnsTimeoutSec = "DnsTimeoutSec";
        public static readonly string ProxyTimeoutSec = "ProxyTimeoutSec";
        public static readonly string KillOnCpuUsage = "KillOnCpuUsage";
        public static readonly string BlockPort80 = "BlockPort80";
        public static readonly string AllowInsecure = "AllowInsecure";
        public static readonly string DNSs = "DNSs";
        public static readonly string CfCleanIP = "CfCleanIP";
        public static readonly string BootstrapIp = "BootstrapIp";
        public static readonly string BootstrapPort = "BootstrapPort";
        public static readonly string ProxyScheme = "ProxyScheme";
        public static readonly string ProxyUser = "ProxyUser";
        public static readonly string ProxyPass = "ProxyPass";
        public static readonly string OnlyBlockedIPs = "OnlyBlockedIPs";
    }

    public readonly struct SSLSetting
    {
        public static readonly string Name = "SSLSetting";
        public static readonly string Enable = "Enable";
        public static readonly string RootCA_Path = "RootCA_Path";
        public static readonly string RootCA_KeyPath = "RootCA_KeyPath";
        public static readonly string Cert_Path = "Cert_Path";
        public static readonly string Cert_KeyPath = "Cert_KeyPath";
        public static readonly string ChangeSni = "ChangeSni";
        public static readonly string DefaultSni = "DefaultSni";
    }

    public readonly struct Programs
    {
        public static readonly string Name = "Programs";

        public readonly struct Fragment
        {
            public static readonly string Name = "Fragment";
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

        public readonly struct DnsRules
        {
            public static readonly string Name = "DnsRules";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string File = "File";
                public static readonly string Text = "Text";
                public static readonly string Disable = "Disable";
            }
            public static readonly string PathOrText = "PathOrText";
        }

        public readonly struct ProxyRules
        {
            public static readonly string Name = "ProxyRules";
            public readonly struct Mode
            {
                public static readonly string Name = "Mode";
                public static readonly string File = "File";
                public static readonly string Text = "Text";
                public static readonly string Disable = "Disable";
            }
            public static readonly string PathOrText = "PathOrText";
        }

    }
}