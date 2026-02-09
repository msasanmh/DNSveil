using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/html/rfc1035#section-3.4.1
public class ARecord : ResourceRecord
{
    public IPAddress IP { get; private set; } = IPAddress.None;

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        result += $"{nameof(IP)}: {IP}\n";
        return result;
    }

    public ARecord() { }

    public ARecord(string domain, uint ttl, IPAddress ipv4)
    {
        Name = domain;
        TYPE = DnsEnums.RRType.A;
        CLASS = DnsEnums.CLASS.IN;
        TimeToLive = ttl;
        TTLDateTime = DateTime.UtcNow;
        IP = ipv4;
    }

    public ARecord(ResourceRecord resourceRecord, IPAddress ipv4)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        IP = ipv4;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos)
    {
        try
        {
            IPAddress ip = new(buffer[pos..(pos + 4)]);
            return new ARecord(resourceRecord, ip);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ARecord Parse: " + ex.Message);
            return new ARecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not ARecord aRecord) return false;
            byte[] ipBytes = new byte[4];
            bool success = aRecord.IP.TryWriteBytes(ipBytes, out _);
            if (success)
            {
                bool rdLengthBool = ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(ipBytes.Length), out byte[] rdLength); // 2 Bytes
                if (!rdLengthBool) return false;
                bufferList.AddRange(rdLength);
                bufferList.AddRange(ipBytes);
                pos += 6;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ARecord TryWrite: " + ex.Message);
            return false;
        }
    }
}