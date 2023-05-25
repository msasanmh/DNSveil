using System;
using System.Net;
using System.Text;

namespace MsmhTools.HTTPProxyServer
{
    /// <summary>
    /// Proxy server settings.
    /// </summary>
    public class ProxySettings
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
    }
}
