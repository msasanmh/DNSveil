using System;
using System.Net;
using System.Text;

namespace MsmhTools.HTTPProxyServer
{
    /// <summary>
    /// System settings.
    /// </summary>
    public class ProxySettings
    {
        /// <summary>
        /// Proxy server settings.
        /// </summary>
        public SettingsProxy Proxy
        {
            get
            {
                return _Proxy;
            }
            set
            {
                if (value == null) _Proxy = new SettingsProxy();
                else _Proxy = value;
            }
        }

        private SettingsProxy _Proxy = new();
    }

    /// <summary>
    /// Proxy server settings.
    /// </summary>
    public class SettingsProxy
    {
        private IPAddress _ListenerIpAddress { get; set; } = IPAddress.Any;
        private int _ListenerPort { get; set; } = 8080;
        private int _MaxThreads { get; set; } = 256;

        /// <summary>
        /// The DNS hostname or IP address on which to listen.
        /// </summary>
        public IPAddress ListenerIpAddress
        {
            get
            {
                return _ListenerIpAddress;
            }
            set
            {
                _ListenerIpAddress = value ?? throw new ArgumentNullException(nameof(ListenerIpAddress));
            }
        }

        /// <summary>
        /// The TCP port on which to listen.
        /// </summary>
        public int ListenerPort
        {
            get
            {
                return _ListenerPort;
            }
            set
            {
                if (value < 0 || value > 65535) throw new ArgumentOutOfRangeException(nameof(ListenerPort));
                _ListenerPort = value;
            }
        }

        /// <summary>
        /// Maximum number of threads to support.
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return _MaxThreads;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxThreads));
                _MaxThreads = value;
            }
        }

        /// <summary>
        /// Enable or disable SSL.
        /// </summary>
        public bool Ssl { get; set; } = false;

        /// <summary>
        /// Enable or disable connections to sites with certificates that cannot be validated.
        /// </summary>
        public bool AcceptInvalidCertificates { get; set; } = true;
    }
}
