namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyTunnelEventArgs : EventArgs
{
    public ProxyTunnelEventArgs(ProxyTunnel pTunnel, byte[] buffer)
    {
        Tunnel = pTunnel;
        Buffer = buffer;
    }

    public ProxyTunnel Tunnel { get; set; }
    public byte[] Buffer { get; set; }
}

public class ProxyRelayEventArgs : EventArgs
{
    public ProxyRelayEventArgs(ProxyRelay pRelay, byte[] buffer)
    {
        Relay = pRelay;
        Buffer = buffer;
    }

    public ProxyRelay Relay { get; set; }
    public byte[] Buffer { get; set; }
}

public class ProxyRelayMITMEventArgs : EventArgs
{
    public ProxyRelayMITMEventArgs(ProxyRelayMITM prm, byte[] buffer)
    {
        RelayMITM = prm;
        Buffer = buffer;
    }

    public ProxyRelayMITM RelayMITM { get; set; }
    public byte[] Buffer { get; set; }
}
