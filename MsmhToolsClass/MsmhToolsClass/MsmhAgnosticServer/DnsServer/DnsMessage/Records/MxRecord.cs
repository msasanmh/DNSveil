using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.9
public class MxRecord : ResourceRecord
{
    public ushort Preference { get; private set; }
    /// <summary>
    /// A host willing to act as a mail exchange
    /// </summary>
    public string Domain { get; private set; } = string.Empty;

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        result += $"{nameof(Preference)}: {Preference}\n";
        result += $"{nameof(Domain)}: {Domain}\n";
        return result;
    }

    public MxRecord() { }

    public MxRecord(string domain, uint ttl, ushort preference, string mailDomain)
    {
        Name = domain;
        TYPE = DnsEnums.RRType.MX;
        CLASS = DnsEnums.CLASS.IN;
        TimeToLive = ttl;
        TTLDateTime = DateTime.UtcNow;
        Preference = preference;
        Domain = mailDomain;
    }

    public MxRecord(ResourceRecord resourceRecord, ushort preference, string domain)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        Preference = preference;
        Domain = domain;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos)
    {
        try
        {
            ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort preference);
            pos += 2;

            string domain = ReadRecordName(buffer, pos, out _, false).ToString();
            if (string.IsNullOrEmpty(domain)) return new MxRecord();
            return new MxRecord(resourceRecord, preference, domain);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS MxRecord Parse: " + ex.Message);
            return new MxRecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, DnsMessage dnsMessage, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not MxRecord mxRecord) return false;
            ByteArrayTool.TryConvertUInt16ToBytes(mxRecord.Preference, out byte[] preferenceArray);
            byte[] domainArray = WriteRecordName(dnsMessage, mxRecord.Domain, pos + 2 + preferenceArray.Length);

            int len = preferenceArray.Length + domainArray.Length;
            bool rdLengthBool = ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(len), out byte[] rdLength); // 2 Bytes
            if (!rdLengthBool) return false;
            bufferList.AddRange(rdLength);
            bufferList.AddRange(preferenceArray);
            bufferList.AddRange(domainArray);
            pos += 2;
            pos += len;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS MxRecord TryWrite: " + ex.Message);
            return false;
        }
    }
}