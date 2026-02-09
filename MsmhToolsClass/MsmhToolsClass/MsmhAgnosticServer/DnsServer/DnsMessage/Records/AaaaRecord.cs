using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/html/rfc3596
public class AaaaRecord : ResourceRecord
{
    public IPAddress IP { get; private set; } = IPAddress.None;

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        result += $"{nameof(IP)}: {IP}\n";
        return result;
    }

    public AaaaRecord() { }

    public AaaaRecord(string domain, uint ttl, IPAddress ipv6)
    {
        Name = domain;
        TYPE = DnsEnums.RRType.AAAA;
        CLASS = DnsEnums.CLASS.IN;
        TimeToLive = ttl;
        TTLDateTime = DateTime.UtcNow;
        IP = ipv6;
    }

    public AaaaRecord(ResourceRecord resourceRecord, IPAddress ipv6)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        IP = ipv6;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos)
    {
        try
        {
            IPAddress ip = new(buffer[pos..(pos + 16)]);
            return new AaaaRecord(resourceRecord, ip);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS AaaaRecord Parse: " + ex.Message);
            return new AaaaRecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not AaaaRecord aaaaRecord) return false;
            byte[] ipBytes = new byte[16];
            bool success = aaaaRecord.IP.TryWriteBytes(ipBytes, out _);
            if (success)
            {
                bool rdLengthBool = ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(ipBytes.Length), out byte[] rdLength); // 2 Bytes
                if (!rdLengthBool) return false;
                bufferList.AddRange(rdLength);
                bufferList.AddRange(ipBytes);
                pos += 18;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS AaaaRecord TryWrite: " + ex.Message);
            return false;
        }
    }
}