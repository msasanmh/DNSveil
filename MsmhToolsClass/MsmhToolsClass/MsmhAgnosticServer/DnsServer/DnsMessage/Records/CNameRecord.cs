using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.1
public class CNameRecord : ResourceRecord
{
    public string CName { get; private set; } = string.Empty;

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        result += $"{nameof(CName)}: {CName}\n";
        return result;
    }

    public CNameRecord() { }

    public CNameRecord(string domain, uint ttl, string cName)
    {
        Name = domain;
        TYPE = DnsEnums.RRType.CNAME;
        CLASS = DnsEnums.CLASS.IN;
        TimeToLive = ttl;
        TTLDateTime = DateTime.UtcNow;
        CName = cName;
    }

    public CNameRecord(ResourceRecord resourceRecord, string domain)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        CName = domain;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos)
    {
        try
        {
            string domain = ReadRecordName(buffer, pos, out _).ToString();
            if (string.IsNullOrEmpty(domain)) return new CNameRecord();
            return new CNameRecord(resourceRecord, domain);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS CNameRecord Parse: " + ex.Message);
            return new CNameRecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, DnsMessage dnsMessage, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not CNameRecord cNameRecord) return false;
            byte[] domainArray = WriteRecordName(dnsMessage, cNameRecord.CName, pos + 2);
            
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
            Debug.WriteLine("DNS CNameRecord TryWrite: " + ex.Message);
            return false;
        }
    }
}