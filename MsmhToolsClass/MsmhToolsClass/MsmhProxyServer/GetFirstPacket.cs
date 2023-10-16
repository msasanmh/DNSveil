using System;
using System.Diagnostics;

namespace MsmhToolsClass.MsmhProxyServer;

public class GetFirstPacket
{
    public byte[] Packet { get; set; } = new byte[MsmhProxyServer.MaxDataSize];
    public Proxy.Name? ProxyName;

    public GetFirstPacket() { }

    public async Task Get(ProxyClient socksClient)
    {
        int recv = await socksClient.ReceiveAsync(Packet);
        if (recv == -1) return; // recv = -1 Will Result in Overflow

        byte[] buffer = new byte[recv];
        Buffer.BlockCopy(Packet, 0, buffer, 0, recv);
        //Debug.WriteLine(BitConverter.ToString(buffer).Replace("-", " "));

        ProxyName = buffer[0] switch
        {
            (byte)Socks.Version.Socks5 => Proxy.Name.Socks5,
            (byte)Socks.Version.Socks4 => Proxy.Name.Socks4,
            0x43 => Proxy.Name.HTTP,
            0x47 => Proxy.Name.HTTP,
            _ => Proxy.Name.HTTP
        };
    }
}