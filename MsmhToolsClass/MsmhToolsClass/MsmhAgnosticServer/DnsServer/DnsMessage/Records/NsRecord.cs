using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.11
public class NsRecord : ResourceRecord
{
    public string NS { get; private set; } = string.Empty;

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        result += $"{nameof(NS)}: {NS}\n";
        return result;
    }

    public NsRecord() { }

    public NsRecord(string domain, uint ttl, string ns)
    {
        Name = domain;
        TYPE = DnsEnums.RRType.NS;
        CLASS = DnsEnums.CLASS.IN;
        TimeToLive = ttl;
        TTLDateTime = DateTime.UtcNow;
        NS = ns;
    }

    public NsRecord(ResourceRecord resourceRecord, string domain)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        NS = domain;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos)
    {
        try
        {
            string domain = ReadRecordName(buffer, pos, out _).ToString();
            if (string.IsNullOrEmpty(domain)) return new NsRecord();
            return new NsRecord(resourceRecord, domain);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS NsRecord Parse: " + ex.Message);
            return new NsRecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, DnsMessage dnsMessage, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not NsRecord nsRecord) return false;
            byte[] domainArray = WriteRecordName(dnsMessage, nsRecord.NS, pos + 2);

            bool rdLengthBool = ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(domainArray.Length), out byte[] rdLength); // 2 Bytes
            if (!rdLengthBool) return false;
            bufferList.AddRange(rdLength);
            bufferList.AddRange(domainArray);
            pos += 2;
            pos += domainArray.Length;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS NsRecord TryWrite: " + ex.Message);
            return false;
        }
    }
}